//지금 열린 세이브 슬롯의 기록을 들고 갱신하는 매니저. 기록이 바뀌면 바로 파일에 저장한다
using System.Collections.Generic;
using UnityEngine;

public class PlayRecordManager : SingletonBase<PlayRecordManager>
{
    public const int SaveSlotCount = 3;
    public const int NoSlotIndex = -1;     //슬롯을 아직 열지 않았을 때
    private const int FirstStageIndex = 0;
    private const long NoLastOpenedTicks = 0; //파일이 없는 슬롯. 실제 연 시각은 늘 이보다 크다
    private const JudgmentGrade UnlockNextStageGrade = JudgmentGrade.Good;  //굿 등급 이상이면 다음 스테이지가 열린다

    private SaveFileService _saveFileService = new SaveFileService();
    private int _currentSlotIndex = NoSlotIndex;
    private SaveSlotModel _currentSaveSlotModel;
    private Dictionary<string, StageRecordModel> _stageRecordModels = new Dictionary<string, StageRecordModel>(); //키는 스테이지Id. 저장용 List에서 만든 찾기용 색인

    private void SaveCurrentSlot()
    {
        _saveFileService.WriteSaveSlot(_currentSlotIndex, _currentSaveSlotModel);
    }

    //불러온 List로 찾기용 Dictionary를 만든다. 키는 늘 모델의 Id에서 꺼낸다
    private void BuildStageRecordIndex()
    {
        _stageRecordModels.Clear();

        foreach (StageRecordModel stageRecordModel in _currentSaveSlotModel.StageRecordModels)
        {
            if (_stageRecordModels.TryAdd(stageRecordModel.StageId, stageRecordModel) == false)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[PlayRecordManager] 같은 스테이지 기록이 두 번 있어 뒤의 것은 무시합니다. {stageRecordModel.StageId}");
#endif
            }
        }
    }

    //처음 기록하는 스테이지면 새로 만들어 저장용 List와 찾기용 Dictionary에 함께 넣는다
    private StageRecordModel GetOrCreateStageRecordModel(string stageId)
    {
        if (_stageRecordModels.TryGetValue(stageId, out StageRecordModel stageRecordModel))
        {
            return stageRecordModel;
        }

        stageRecordModel = new StageRecordModel(stageId);
        _currentSaveSlotModel.AddStageRecordModel(stageRecordModel);
        _stageRecordModels.Add(stageRecordModel.StageId, stageRecordModel);

        return stageRecordModel;
    }
    
    //다음 스테이지 기록을 만들어 연다. 마지막 스테이지면 열 것이 없다. 이미 열려 있으면 그대로 둔다
    private void UnlockNextStage(string stageId)
    {
        string nextStageId = GameDataManager.Instance.FindNextStageId(stageId);

        if (string.IsNullOrEmpty(nextStageId))
        {
            return;
        }

        GetOrCreateStageRecordModel(nextStageId).Unlock();
    }

    //빈 슬롯이면 새로 만든다. 연 시각을 남겨 이어하기가 이 슬롯을 고르게 한다
    public void LoadSaveSlot(int slotIndex)
    {
        SaveSlotModel saveSlotModel = _saveFileService.ReadSaveSlot(slotIndex);
        if (saveSlotModel == null)
        {
            saveSlotModel = new SaveSlotModel();
        }

        _currentSlotIndex = slotIndex;
        _currentSaveSlotModel = saveSlotModel;
        BuildStageRecordIndex();

        _currentSaveSlotModel.UpdateLastOpenedTicks();
        SaveCurrentSlot();
    }

    public void IncreaseStageTryCount(string stageId)
    {
        GetOrCreateStageRecordModel(stageId).IncreaseTryCount();
        SaveCurrentSlot();
    }

    public void UpdateStageBestRecord(string stageId, JudgmentGrade judgmentGrade, int timingErrorMs)
    {
        GetOrCreateStageRecordModel(stageId).UpdateBestRecord(judgmentGrade, timingErrorMs);
        if (judgmentGrade >= UnlockNextStageGrade)
        {
            UnlockNextStage(stageId);
        }

        SaveCurrentSlot();
    }

    public void SetLastPlayedStageId(string stageId)
    {
        _currentSaveSlotModel.SetLastPlayedStageId(stageId);
        SaveCurrentSlot();
    }

    //아래 조회 메서드는 묻기만 하므로 기록을 새로 만들지 않는다
    public int GetStageTryCount(string stageId)
    {
        if (_stageRecordModels.TryGetValue(stageId, out StageRecordModel stageRecordModel))
        {
            return stageRecordModel.TryCount;
        }

        return 0;
    }

    public JudgmentGrade GetStageBestGrade(string stageId)
    {
        if (_stageRecordModels.TryGetValue(stageId, out StageRecordModel stageRecordModel))
        {
            return stageRecordModel.BestGrade;
        }

        return JudgmentGrade.None;
    }

    public int GetStageBestTimingErrorMs(string stageId)
    {
        if (_stageRecordModels.TryGetValue(stageId, out StageRecordModel stageRecordModel))
        {
            return stageRecordModel.BestTimingErrorMs;
        }

        return StageManager.NoInputTimingErrorMs;
    }

    //첫 스테이지는 열어 줄 앞 스테이지가 없어 기록과 상관없이 늘 열려 있다
    public bool IsStageUnlocked(string stageId)
    {
        if (stageId == GameDataManager.Instance.StageDataList[FirstStageIndex].Id)
        {
            return true;
        }

        if (_stageRecordModels.TryGetValue(stageId, out StageRecordModel stageRecordModel))
        {
            return stageRecordModel.IsUnlocked;
        }

        return false;
    }

    //열린 슬롯을 바꾸지 않고 파일을 읽기만 한다. 슬롯 카드와 이어하기에서 쓴다
    public SaveSlotSummary GetSaveSlotSummary(int slotIndex)
    {
        SaveSlotModel saveSlotModel = _saveFileService.ReadSaveSlot(slotIndex);
        if (saveSlotModel == null)
        {
            return new SaveSlotSummary(false, NoLastOpenedTicks);
        }

        return new SaveSlotSummary(true, saveSlotModel.LastOpenedTicks);
    }

    //이어하기 버튼을 누를 수 있는지. 파일이 있는지만 보므로 읽지 않는다
    public bool HasAnySaveFile()
    {
        for (int i = 0; i < SaveSlotCount; i++)
        {
            if (_saveFileService.HasSaveFile(i))
            {
                return true;
            }
        }

        return false;
    }

    //세 슬롯 중 마지막으로 연 슬롯. 세이브가 하나도 없으면 NoSlotIndex
    public int FindLastOpenedSlotIndex()
    {
        int lastOpenedSlotIndex = NoSlotIndex;
        long lastOpenedTicks = NoLastOpenedTicks;

        for (int i = 0; i < SaveSlotCount; i++)
        {
            SaveSlotSummary saveSlotSummary = GetSaveSlotSummary(i);
            if (saveSlotSummary.HasSaveFile && saveSlotSummary.LastOpenedTicks > lastOpenedTicks)
            {
                lastOpenedSlotIndex = i;
                lastOpenedTicks = saveSlotSummary.LastOpenedTicks;
            }
        }

        return lastOpenedSlotIndex;
    }

    //새 슬롯이라 아직 플레이한 스테이지가 없으면 첫 스테이지
    public string GetLastPlayedStageId()
    {
        string lastPlayedStageId = _currentSaveSlotModel.LastPlayedStageId;
        if (string.IsNullOrEmpty(lastPlayedStageId))
        {
            return GameDataManager.Instance.StageDataList[FirstStageIndex].Id;
        }

        return lastPlayedStageId;
    }
}
