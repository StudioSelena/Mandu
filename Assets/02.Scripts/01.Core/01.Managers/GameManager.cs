//게임 전체 흐름(스테이지 시작과 다음 스테이지 전환)을 관리하는 매니저
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

//다른 매니저(기본 0)가 모두 이벤트 구독을 마친 뒤 첫 스테이지를 시작하도록 늦게 실행한다
[DefaultExecutionOrder(100)]
public class GameManager : SingletonBase<GameManager>
{
    private const int FirstStageIndex = 0;
    private const int InvalidStageIndex = -1; //Id로 찾지 못한 스테이지
    //화면에 보이는 스테이지 번호는 1부터 센다. 인덱스는 0부터라 1을 더한다
    public const int StageNumberOffset = 1;
    private const string PlayerManduAddress = "PlayerMandu"; //메인만두 프리팹 어드레서블 주소
    private const string BackgroundAddress = "Background"; //망점 배경 프리팹 어드레서블 주소

    private int _currentStageIndex = FirstStageIndex;
    private Dictionary<string, StageRecordModel> _stageRecordModels = new Dictionary<string, StageRecordModel>();
    private int _playerManduInstanceId = GameObjectManager.InvalidInstanceId; //플레이 구간 동안 쓰는 메인 만두 번호. 없앨 때 쓴다
    private int _backgroundInstanceId = GameObjectManager.InvalidInstanceId; //게임 시작부터 끝까지 쓰는 배경 번호. 로비가 생겨도 그대로 둔다
    private bool _isInGamePlay = false; //플레이 구간 안인지. 나가기를 두 번 눌러도 한 번만 나가고 이미 반납한 등급 이미지를 종료 때 또 반납하지 않게 한다
    private UniTaskCompletionSource _sabotagePreparedSource; //다음 스테이지 준비를 기다리는 동안만 있다. 없을 때 온 준비 끝을 무시(다시하기, 개발자 모드)

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent += HandleStageStateChanged;
        SabotageManager.Instance.SabotagePreparedEvent += HandleSabotagePrepared;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        InputManager.Instance.ToggleDevModeEvent += HandleToggleDevMode;
#endif
        InitiateGame().Forget();
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        //나가기로 이미 반납했으면 다시 반납하지 않는다
        if (_isInGamePlay)
        {
            ReleaseAllJudgmentGradeSprites();
        }

        DestroyPlayerMandu();
        DestroyBackground();

        if (SabotageManager.Instance != null)
        {
            SabotageManager.Instance.SabotagePreparedEvent -= HandleSabotagePrepared;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (InputManager.Instance != null)
        {
            InputManager.Instance.ToggleDevModeEvent -= HandleToggleDevMode;
        }
#endif

        if (StageManager.Instance == null)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent -= HandleStageStateChanged;
    }

    public void RequestNextStage()
    {
        TransitionToNextStage().Forget();
    }

    public void RequestRetryStage()
    {
        UIManager.Instance.CloseUI(UIType.ResultUI);
        PrepareCurrentStage();
    }

    public void RequestExitGamePlay()
    {
        if (_isInGamePlay == false)
        {
            return;
        }

        ExitGamePlay().Forget();
    }

    public void RequestOpenStageSelect()
    {
        UIManager.Instance.CloseUI(UIType.LobbyUI);
        UIManager.Instance.OpenUI(UIRootType.Main, UIType.StageSelectUI);
    }

    public void RequestOpenLobby()
    {
        UIManager.Instance.CloseUI(UIType.StageSelectUI);
        UIManager.Instance.OpenUI(UIRootType.Main, UIType.LobbyUI);
    }

    //스테이지 선택창에서 고른 스테이지로 플레이에 들어간다. 김이 덮이는 동안 또 누르면 무시한다
    public void RequestEnterGamePlay(string stageId)
    {
        if (_isInGamePlay || UIManager.Instance.IsOpenedUI(UIType.TransitionUI))
        {
            return;
        }

        int stageIndex = FindStageIndex(stageId);
        if (stageIndex == InvalidStageIndex)
        {
            return;
        }

        _currentStageIndex = stageIndex;
        EnterGamePlay().Forget();
    }

    public void RequestQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    //개발자 패널에서 고른 스테이지로 바로 시작한다. 삶는 중이어도 각 매니저가 Ready 에서 정리한다
    public void RequestStartStage(int stageIndex)
    {
        if (stageIndex < FirstStageIndex || stageIndex >= GameDataManager.Instance.StageDataList.Count)
        {
            return;
        }

        UIManager.Instance.CloseUI(UIType.ResultUI);
        _currentStageIndex = stageIndex;
        PrepareCurrentStage();
    }

    //백틱 키로 개발자 패널을 열고 닫는다
    private void HandleToggleDevMode()
    {
        if (UIManager.Instance.IsOpenedUI(UIType.DevModeUI))
        {
            UIManager.Instance.CloseUI(UIType.DevModeUI);
            return;
        }

        UIManager.Instance.OpenUI(UIRootType.VeryFront, UIType.DevModeUI);
    }
#endif

    private void PrepareCurrentStage()
    {
        StageManager.Instance.PrepareStage(GetCurrentStageId(), GetCurrentStageNumber());
    }

    //김으로 덮은 뒤 결과 화면을 닫고 다음 스테이지를 준비한다. 준비가 끝나면 김을 걷는다
    private async UniTaskVoid TransitionToNextStage()
    {
        TransitionUI transitionUI = OpenTransitionUI();
        if (transitionUI == null)
        {
            return;
        }

        await transitionUI.CoverAsync();

        UIManager.Instance.CloseUI(UIType.ResultUI);
        AdvanceToNextStageIndex();
        await PrepareCurrentStageAndWaitAsync();

        await UncoverAndCloseTransitionAsync(transitionUI);
    }

    //마지막 스테이지 다음은 첫 스테이지로 돌아간다
    private void AdvanceToNextStageIndex()
    {
        _currentStageIndex++;
        if (_currentStageIndex >= GameDataManager.Instance.StageDataList.Count)
        {
            _currentStageIndex = FirstStageIndex;
        }
    }

    //스테이지를 준비하고 방해요소 준비 끝까지 기다린다. 방해요소가 없으면 준비 한 줄 안에서 바로 끝이 오므로 대상을 먼저 만든다
    private async UniTask PrepareCurrentStageAndWaitAsync()
    {
        _sabotagePreparedSource = new UniTaskCompletionSource();
        PrepareCurrentStage();

        await _sabotagePreparedSource.Task;
        _sabotagePreparedSource = null;
    }

    private void HandleSabotagePrepared()
    {
        if (_sabotagePreparedSource == null)
        {
            return;
        }

        _sabotagePreparedSource.TrySetResult();
    }

    private TransitionUI OpenTransitionUI()
    {
        UIBase openedUI = UIManager.Instance.OpenUI(UIRootType.VeryFront, UIType.TransitionUI);
        return openedUI as TransitionUI;
    }

    private async UniTask UncoverAndCloseTransitionAsync(TransitionUI transitionUI)
    {
        await transitionUI.UncoverAsync();
        UIManager.Instance.CloseUI(UIType.TransitionUI);
    }

    //스테이지 Id가 엑셀 순서로 몇 번째인지 찾는다. 기록은 Id로, 진행은 인덱스로 다루기 때문에 둘을 잇는다
    private int FindStageIndex(string stageId)
    {
        IReadOnlyList<StageData> stageDataList = GameDataManager.Instance.StageDataList;
        for (int i = 0; i < stageDataList.Count; i++)
        {
            if (stageDataList[i].Id == stageId)
            {
                return i;
            }
        }

        return InvalidStageIndex;
    }

    public string GetCurrentStageId()
    {
        return GameDataManager.Instance.StageDataList[_currentStageIndex].Id;
    }

    private int GetCurrentStageNumber()
    {
        return _currentStageIndex + StageNumberOffset;
    }

    private void HandleStageStateChanged(StageState stageState)
    {
        switch (stageState)
        {
            case StageState.Boiling:
                IncreaseCurrentStageTryCount();
                break;

            case StageState.ShowingResult:
                OpenResultUI();
                break;
        }
    }

    private void IncreaseCurrentStageTryCount()
    {
        StageRecordModel stageRecordModel = GetStageRecordModel(GetCurrentStageId());
        stageRecordModel.IncreaseTryCount();
    }

    //처음 도전하는 스테이지면 기록을 새로 만든다
    private StageRecordModel GetStageRecordModel(string stageId)
    {
        if (_stageRecordModels.TryGetValue(stageId, out StageRecordModel stageRecordModel))
        {
            return stageRecordModel;
        }

        stageRecordModel = new StageRecordModel();
        _stageRecordModels.Add(stageId, stageRecordModel);
        return stageRecordModel;
    }

    //기록이 없는 스테이지는 0회다. 묻기만 하는 메서드라 기록을 새로 만들지 않는다
    public int GetStageTryCount(string stageId)
    {
        if (_stageRecordModels.TryGetValue(stageId, out StageRecordModel stageRecordModel))
        {
            return stageRecordModel.TryCount;
        }

        return 0;
    }

    private void OpenResultUI()
    {
        UIBase openedUI = UIManager.Instance.OpenUI(UIRootType.Popup, UIType.ResultUI);
        ResultUI resultUI = openedUI as ResultUI;
        if (resultUI == null)
        {
            return;
        }

        StageManager stageManager = StageManager.Instance;
        StagePlayResult stagePlayResult = new StagePlayResult(
            judgmentGrade: stageManager.CurrentJudgmentGrade,
            targetTimeMs: stageManager.CurrentTargetTimeMs,
            inputTimeMs: stageManager.CurrentInputTimeMs,
            timingErrorMs: stageManager.CurrentTimingErrorMs);

        int tryCount = GetStageRecordModel(GetCurrentStageId()).TryCount;
        resultUI.SetStageResult(stagePlayResult, tryCount);
    }

    //플레이 구간 동안 결과 화면에서 쓸 등급 이미지를 미리 불러둔다. 플레이 나가기에서 반납한다
    private void LoadAllJudgmentGradeSprites()
    {
        foreach (JudgmentGrade judgmentGrade in Enum.GetValues(typeof(JudgmentGrade)))
        {
            if (judgmentGrade == JudgmentGrade.None)
            {
                continue;
            }

            string address = GameUtil.GetJudgmentGradeAddress(judgmentGrade);
            ResourceManager.Instance.LoadAsset<Sprite[]>(address).Forget();
        }
    }

    private void ReleaseAllJudgmentGradeSprites()
    {
        if (ResourceManager.Instance == null)
        {
            return;
        }

        foreach (JudgmentGrade judgmentGrade in Enum.GetValues(typeof(JudgmentGrade)))
        {
            if (judgmentGrade == JudgmentGrade.None)
            {
                continue;
            }

            ResourceManager.Instance.Release(GameUtil.GetJudgmentGradeAddress(judgmentGrade));
        }
    }

    //김으로 덮고 등급 이미지와 메인 만두를 준비한 뒤 스테이지 화면을 연다. 스테이지 준비가 끝나면 김을 걷는다
    private async UniTaskVoid EnterGamePlay()
    {
        TransitionUI transitionUI = OpenTransitionUI();
        if (transitionUI == null)
        {
            return;
        }

        await transitionUI.CoverAsync();

        UIManager.Instance.CloseUI(UIType.StageSelectUI);
        _isInGamePlay = true;
        LoadAllJudgmentGradeSprites();
        await SpawnPlayerMandu();

        UIManager.Instance.OpenUI(UIRootType.Main, UIType.StageUI);
        await PrepareCurrentStageAndWaitAsync();

        await UncoverAndCloseTransitionAsync(transitionUI);
    }

    //김으로 덮고 화면과 방해요소를 닫은 뒤 메인 만두와 등급 이미지를 반납한다. 쓰는 쪽을 먼저 닫고 빌려준 리소스는 나중에 반납한다
    private async UniTaskVoid ExitGamePlay()
    {
        TransitionUI transitionUI = OpenTransitionUI();
        if (transitionUI == null)
        {
            return;
        }

        //덮는 동안 나가기를 또 눌러도 RequestExitGamePlay에서 막히도록 먼저 바꾼다
        _isInGamePlay = false;
        await transitionUI.CoverAsync();

        UIManager.Instance.CloseUI(UIType.ResultUI);
        UIManager.Instance.CloseUI(UIType.StageUI);
        SabotageManager.Instance.CloseCurrentSabotage();
        DestroyPlayerMandu();
        ReleaseAllJudgmentGradeSprites();

        UIManager.Instance.OpenUI(UIRootType.Main, UIType.StageSelectUI);
        await UncoverAndCloseTransitionAsync(transitionUI);
    }

    //김으로 덮은 화면에서 시작해 배경을 만든다. 글자 준비가 끝나면 김을 걷는다
    private async UniTaskVoid InitiateGame()
    {
        TransitionUI transitionUI = OpenTransitionUI();
        if (transitionUI == null)
        {
            return;
        }

        transitionUI.CoverImmediately();

        await SpawnBackground();
        //글자 준비 중에 김을 걷으면 원래 글자가 잠깐 보인다
        await LocalizationSettings.InitializationOperation.ToUniTask();

        UIManager.Instance.OpenUI(UIRootType.Main, UIType.LobbyUI);
        await UncoverAndCloseTransitionAsync(transitionUI);
    }

    private async UniTask SpawnPlayerMandu()
    {
        _playerManduInstanceId = await GameObjectManager.Instance.RequestSpawnWorldObject(PlayerManduAddress);
    }

    private void DestroyPlayerMandu()
    {
        if (GameObjectManager.Instance == null)
        {
            return;
        }

        //생성에 실패했거나 끝나기 전이면 없앨 것이 없다
        if (_playerManduInstanceId == GameObjectManager.InvalidInstanceId)
        {
            return;
        }

        GameObjectManager.Instance.RequestDestroyWorldObject(_playerManduInstanceId);
        _playerManduInstanceId = GameObjectManager.InvalidInstanceId;
    }

    private async UniTask SpawnBackground()
    {
        _backgroundInstanceId = await GameObjectManager.Instance.RequestSpawnWorldObject(BackgroundAddress);
    }

    private void DestroyBackground()
    {
        if (GameObjectManager.Instance == null)
        {
            return;
        }

        //생성에 실패했거나 끝나기 전이면 없앨 것이 없다
        if (_backgroundInstanceId == GameObjectManager.InvalidInstanceId)
        {
            return;
        }

        GameObjectManager.Instance.RequestDestroyWorldObject(_backgroundInstanceId);
        _backgroundInstanceId = GameObjectManager.InvalidInstanceId;
    }
}
