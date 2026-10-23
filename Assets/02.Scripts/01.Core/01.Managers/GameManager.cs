//게임 전체 흐름(스테이지 시작과 다음 스테이지 전환)을 관리하는 매니저
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

//다른 매니저(기본 0)가 모두 이벤트 구독을 마친 뒤 첫 스테이지를 시작하도록 늦게 실행한다
[DefaultExecutionOrder(100)]
public class GameManager : SingletonBase<GameManager>
{
    private const int FirstStageIndex = 0;
    //화면에 보이는 스테이지 번호는 1부터 센다. 인덱스는 0부터라 1을 더한다
    private const int StageNumberOffset = 1;

    private int _currentStageIndex = FirstStageIndex;
    private Dictionary<string, StageRecordModel> _stageRecordModels = new Dictionary<string, StageRecordModel>();

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent += HandleStageStateChanged;
        LoadAllJudgmentGradeSprites();

        UIManager.Instance.OpenUI(UIRootType.Main, UIType.StageUI);
        PrepareCurrentStage();
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        ReleaseAllJudgmentGradeSprites();

        if (StageManager.Instance == null)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent -= HandleStageStateChanged;
    }

    public void RequestNextStage()
    {
        UIManager.Instance.CloseUI(UIType.ResultUI);

        _currentStageIndex++;
        if (_currentStageIndex >= GameDataManager.Instance.StageDataList.Count)
        {
            _currentStageIndex = FirstStageIndex;
        }

        PrepareCurrentStage();
    }

    public void RequestRetryStage()
    {
        UIManager.Instance.CloseUI(UIType.ResultUI);
        PrepareCurrentStage();
    }

    private void PrepareCurrentStage()
    {
        StageManager.Instance.PrepareStage(GetCurrentStageId(), GetCurrentStageNumber());
    }

    private string GetCurrentStageId()
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

    //플레이 구간 동안 결과 화면에서 쓸 등급 이미지를 미리 불러둔다. 로비가 생기면 해제 시점을 로비로 나갈 때로 옮김
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
}
