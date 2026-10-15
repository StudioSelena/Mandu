//게임 전체 흐름(스테이지 시작과 다음 스테이지 전환)을 관리하는 매니저
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    private const int FirstStageIndex = 0;

    private int _currentStageIndex = FirstStageIndex;

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent += HandleStageStateChanged;

        UIManager.Instance.OpenUI(UIRootType.Main, UIType.StageUI);
        PrepareCurrentStage();
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        if(StageManager.Instance == null)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent -= HandleStageStateChanged;
    }

    private void PrepareCurrentStage()
    {
        string stageId = GameDataManager.Instance.StageDataList[_currentStageIndex].Id;
        StageManager.Instance.PrepareStage(stageId);
    }

    private void HandleStageStateChanged(StageState stageState)
    {
        if (stageState != StageState.ShowingResult)
        {
            return;
        }

        UIManager.Instance.OpenUI(UIRootType.Popup, UIType.ResultUI);
    }
}
