using UnityEngine;

public static class GameUtil
{
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