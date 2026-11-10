//스테이지 한 판의 진행 상태. StageManager만 바꿀 수 있다
public enum StageState
{
    None,
    Ready,
    Boiling,
    JudgmentComplete,
    ShowingResult,
}