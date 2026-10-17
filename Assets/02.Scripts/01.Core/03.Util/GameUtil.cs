//상태 없는 게임 계산 함수 모음 (판정, 등급 이미지 주소)
using UnityEngine;

public static class GameUtil
{
    //등급 이미지 어드레서블 주소 앞부분. 뒤에 등급 이름을 붙인다. (예: JudgmentGrade_Perfect)
    private const string JudgmentGradeAddressPrefix = "JudgmentGrade_";

    //삶기 버튼부터 흐른 초와 목표 시간(ms)의 차이를 밀리초 int로 돌려준다
    public static int CalculateTimingErrorMs(double elapsedSeconds, int targetTimeMs)
    {
        double elapsedMs = elapsedSeconds * TimeUtil.MillisecondsPerSecond;
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

    //등급에 맞는 등급 이미지 어드레서블 주소를 돌려준다
    public static string GetJudgmentGradeAddress(JudgmentGrade judgmentGrade)
    {
        return JudgmentGradeAddressPrefix + judgmentGrade.ToString();
    }
}