using System.Globalization;
using UnityEngine;

public static class GameUtil
{
    //초를 밀리초로 바꿀 때 곱하는 값
    public const int MillisecondsPerSecond = 1000;

    //화면에 초를 보여줄 때 형식 (예: 10.00)
    private const string SecondsDisplayFormat = "0.00";

    //입력이 없을 때 초 자리에 보여줄 글자
    private const string NoInputSecondsDisplayText = "-.---";

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

    //밀리초를 화면 표시용 초 문자열로 돌려준다. 지역 설정과 상관없이 소수점은 마침표로 표시한다
    public static string GetSecondsDisplayText(int timeMs)
    {
        double timeSeconds = (double)timeMs / MillisecondsPerSecond;
        return timeSeconds.ToString(SecondsDisplayFormat, CultureInfo.InvariantCulture);
    }

    //입력 시간(ms)을 화면 표시용 문자열로 돌려준다. 입력이 없으면 -.--
    public static string GetInputTimeDisplayText(int inputTimeMs)
    {
        if (inputTimeMs == StageManager.NoInputTimeMs)
        {
            return NoInputSecondsDisplayText;
        }

        return GetSecondsDisplayText(inputTimeMs);
    }
}