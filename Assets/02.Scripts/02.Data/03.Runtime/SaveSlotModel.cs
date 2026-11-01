//세이브 슬롯 하나에 저장하는 값(마지막 플레이 스테이지, 마지막으로 연 시각, 스테이지 기록 목록)을 담는 모델
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveSlotModel
{
    [SerializeField] private string _lastPlayedStageId; //스테이지 선택창에 들어갔을 때 골라둘 줄
    [SerializeField] private long _lastOpenedTicks;     //이어하기에서 슬롯끼리 비교한다. JsonUtility가 DateTime을 저장하지 못해 Ticks로 둔다
    [SerializeField] private List<StageRecordModel> _stageRecordModels = new List<StageRecordModel>();
    
    public string LastPlayedStageId
    {
        get { return _lastPlayedStageId; }
    }

    public long LastOpenedTicks
    {
        get { return _lastOpenedTicks; }
    }

    public IReadOnlyList<StageRecordModel> StageRecordModels
    {
        get { return _stageRecordModels; }
    }

    public void SetLastPlayedStageId(string stageId)
    {
        _lastPlayedStageId = stageId;
    }

    public void UpdateLastOpenedTicks()
    {
        _lastOpenedTicks = DateTime.UtcNow.Ticks;
    }

    public void AddStageRecordModel(StageRecordModel stageRecordModel)
    {
        _stageRecordModels.Add(stageRecordModel);
    }
}