//스테이지 한 판의 판정 결과(등급, 목표시간, 입력시간, 오차)를 묶는 값 타입
public readonly struct StagePlayResult
{
    public JudgmentGrade JudgmentGrade { get; }
    public int TargetTimeMs { get; }
    public int InputTimeMs { get; }
    public int TimingErrorMs { get; }

    public StagePlayResult(JudgmentGrade judgmentGrade, int targetTimeMs, int inputTimeMs, int timingErrorMs)
    {
        JudgmentGrade = judgmentGrade;
        TargetTimeMs = targetTimeMs;
        InputTimeMs = inputTimeMs;
        TimingErrorMs = timingErrorMs;
    }
}