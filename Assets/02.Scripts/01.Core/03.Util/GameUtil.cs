using UnityEngine;

public static class GameUtil
{
    //초를 밀리초로 바꿀 때 곱하는 값
    public const int MillisecondsPerSecond = 1000;

    //삶기 버튼부터 흐른 초와 목표 시간(ms)의 차이를 밀리초 int로 돌려준다
    public static int CalculateTimingErrorMs(double elapsedSeconds, int targetTimeMs)
    {
        double elapsedMs = elapsedSeconds * MillisecondsPerSecond;
        double differenceMs = elapsedMs - targetTimeMs;
        double absoluteErrorMs = System.Math.Abs(differenceMs);
        return (int)absoluteErrorMs;
    }

    //오차 밀리초를 기반으로 판정 등급을 돌려준다
    public static JudgmentGrade JudgeTimingGrade(int timingErrorMs, TargetTimeData targetTimeData)
    {
        if (targetTimeData == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[GameUtil] TargetTimeData가 null입니다.");
#endif
            return JudgmentGrade.None;
        }
        
        if (timingErrorMs == 0)
        {
            return JudgmentGrade.Monster;
        }
        
        if (timingErrorMs <= targetTimeData.PerfectWindowMs)
        {
            return JudgmentGrade.Perfect;
        }

        if (timingErrorMs <= targetTimeData.NiceWindowMs)
        {
            return JudgmentGrade.Nice;
        }

        if (timingErrorMs <= targetTimeData.GoodWindowMs)
        {
            return JudgmentGrade.Good;
        }

        return JudgmentGrade.Fail;
    }
}