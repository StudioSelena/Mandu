//게임 전체 흐름(스테이지 시작과 다음 스테이지 전환)을 관리하는 매니저
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    private const int FirstStageIndex = 0;

    private int _currentStageIndex = FirstStageIndex;
    private Dictionary<string, StageRecordModel> _stageRecordModels = new Dictionary<string, StageRecordModel>();

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
        StageManager.Instance.PrepareStage(GetCurrentStageId());
    }

    private string GetCurrentStageId()
    {
        return GameDataManager.Instance.StageDataList[_currentStageIndex].Id;
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
}
