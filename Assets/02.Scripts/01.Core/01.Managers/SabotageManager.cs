//스테이지 상태에 맞춰 방해요소를 열고, 시작하고, 멈추고, 닫는 매니저
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SabotageManager : SingletonBase<SabotageManager>
{
    private string _currentSabotageId; //다시하기로 같은 방해요소인지 비교하고, 닫을 때 종류를 조회한다
    private ISabotage _currentSabotage;

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent += HandleStageStateChanged;
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        if (StageManager.Instance == null)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent -= HandleStageStateChanged;
    }

    private void HandleStageStateChanged(StageState stageState)
    {
        switch (stageState)
        {
            case StageState.Ready:
                //삶는 도중 스테이지가 바뀌면 멈추지 않은 방해요소가 남아 있으므로 준비 전에 먼저 멈춘다
                StopCurrentSabotage();
                PrepareCurrentSabotage().Forget();
                break;
            
            case StageState.Boiling:
                StartCurrentSabotage();
                break;

            case StageState.JudgmentComplete:
                StopCurrentSabotage();
                break;
        }
    }

    //새 스테이지의 방해요소를 준비한다. 같은 종류면 재사용하고, 다르면 이전 것을 닫고 새로 연다
    private async UniTaskVoid PrepareCurrentSabotage()
    {
        string newSabotageId = StageManager.Instance.CurrentSabotageId;

        //1.다시하기처럼 같은 방해요소면 다시 준비만 한다
        if (_currentSabotage != null && newSabotageId == _currentSabotageId)
        {
            _currentSabotage.PrepareSabotage(newSabotageId);
            return;
        }

        //2.새 Id가 비었는지 보기 전에 이전것부터 닫는다. 순서가 바뀌면 이전 방해요소가 화면에 남는다
        CloseCurrentSabotage();
        
        //3. 방해요소 없는 스테이지
        if (string.IsNullOrEmpty(newSabotageId))
        {
            return;
        }

        //4.새로 열고 준비한다
        ISabotage openedSabotage = await OpenSabotage(newSabotageId);
        if (openedSabotage == null)
        {
            return;
        }

        _currentSabotageId = newSabotageId;
        _currentSabotage = openedSabotage;
        _currentSabotage.PrepareSabotage(newSabotageId);
    }

    //종류에 맞는 곳에서 방해요소를 연다. 방해요소 종류가 늘면 여기와 CloseCurrentSabotage에 case를 더한다
    private async UniTask<ISabotage> OpenSabotage(string sabotageId)
    {
        SabotageData sabotageData = GameDataManager.Instance.GetSabotageData(sabotageId);
        if (sabotageData == null)
        {
            return null;
        }

        ISabotage openedSabotage = null;

        switch (sabotageData.Type)
        {
            case SabotageType.FakeNumber:
                UIBase openedUI = UIManager.Instance.OpenUI(UIRootType.Content, UIType.FakeNumberSabotageUI, false);
                openedSabotage = openedUI as ISabotage;
                break;
        }

        if (openedSabotage == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"[SabotageManager] {sabotageId}를 열지 못했습니다. 종류 {sabotageData.Type}");
#endif
        }

        return openedSabotage;
    }

    //지금 열린 방해요소를 종류에 맞는 곳에서 닫고 기억을 비운다
    private void CloseCurrentSabotage()
    {
        if (_currentSabotage == null)
        {
            return;
        }

        SabotageData sabotageData = GameDataManager.Instance.GetSabotageData(_currentSabotageId);
        if (sabotageData != null)
        {
            switch (sabotageData.Type)
            {
                case SabotageType.FakeNumber:
                    UIManager.Instance.CloseUI(UIType.FakeNumberSabotageUI);
                    break;
            }
        }

        _currentSabotageId = null;
        _currentSabotage = null;
    }

    private void StartCurrentSabotage()
    {
        if (_currentSabotage == null)
        {
            return;
        }

        _currentSabotage.StartSabotage();
    }

    private void StopCurrentSabotage()
    {
        if (_currentSabotage == null)
        {
            return;
        }

        _currentSabotage.StopSabotage();
    }
}