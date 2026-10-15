//결과 화면. 등급, 오차, 다음 버튼 등을 맡는 UI
using TMPro;
using UnityEngine;

public class ResultUI : UIBase
{
    private const string TimingErrorFormat = "0.000";
    private const string NoInputTimingErrorText = "-.---";

    [SerializeField] private TMP_Text Text_Grade;
    [SerializeField] private TMP_Text Text_TimingError;
    [SerializeField] private UIButton Button_Next;

    private void OnEnable()
    {
        Button_Next.BindOnClickButtonEvent(OnClick_Next);
    }

    public void SetStageResult(JudgmentGrade judgmentGrade, int timingErrorMs)
    {
        Text_Grade.text = judgmentGrade.ToString();
        Text_TimingError.text = GetTimingErrorDisplayText(timingErrorMs);
    }

    private string GetTimingErrorDisplayText(int timingErrorMs)
    {
        if (timingErrorMs == StageManager.NoInputTimingErrorMs)
        {
            return NoInputTimingErrorText;
        }

        double timingErrorSeconds = (double)timingErrorMs / GameUtil.MillisecondsPerSecond;
        return timingErrorSeconds.ToString(TimingErrorFormat);
    }

    private void OnClick_Next()
    {
        GameManager.Instance.RequestNextStage();
    }
}