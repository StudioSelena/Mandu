//세이브 슬롯 카드에 보여줄 요약 값(파일이 있는지, 마지막으로 연 시각, 진행 스테이지, 별 합계)을 묶는 값 타입. PlayRecordManager가 계산해 넘긴다

public readonly struct SaveSlotSummary
{
    public bool HasSaveFile { get; }
    public long LastOpenedTicks { get; }
    public int UnlockedStageCount { get; }
    public int StageCount { get; }
    public int EarnedStarCount { get; }
    public int MaxEarnedStarCount { get; }

    public SaveSlotSummary(bool hasSaveFile, long lastOpenedTicks, int unlockedStageCount, int stageCount, int earnedStarCount, int maxEarnedStarCount)
    {
        HasSaveFile = hasSaveFile;
        LastOpenedTicks = lastOpenedTicks;
        UnlockedStageCount = unlockedStageCount;
        StageCount = stageCount;
        EarnedStarCount = earnedStarCount;
        MaxEarnedStarCount = maxEarnedStarCount;
    }
}
