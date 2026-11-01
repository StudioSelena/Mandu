//세이브 슬롯 카드에 보여줄 요약 값(파일이 있는지, 마지막으로 연 시각)을 묶는 값 타입. PlayRecordManager가 계산해 넘긴다

public readonly struct SaveSlotSummary
{
    public bool HasSaveFile { get; }
    public long LastOpenedTicks { get; }

    public SaveSlotSummary(bool hasSaveFile, long lastOpenedTicks)
    {
        HasSaveFile = hasSaveFile;
        LastOpenedTicks = lastOpenedTicks;
    }
}
