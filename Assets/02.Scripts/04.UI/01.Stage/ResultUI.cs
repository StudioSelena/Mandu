//결과 화면. 등급, 기록, 다시하기와 다음 버튼 등을 맡는 UI
using System.Globalization;
using TMPro;
using UnityEngine;

public class ResultUI : UIBase
{
    private const string SecondsTimeFormat = "0.00";
    private const string NoInputTimeText = "-.--";
    private const string TimingErrorUnitText = "MS";
    private const string NoInputTimingErrorText = "-";

    [SerializeField] private TMP_Text Text_Grade;
    [SerializeField] private TMP_Text Text_TargetTime;
    [SerializeField] private TMP_Text Text_InputTime;
    [SerializeField] private TMP_Text Text_TimingError;
    [SerializeField] private UIButton Button_Retry;
    [SerializeField] private UIButton Button_Next;

    private void OnEnable()
    {
        Button_Retry.BindOnClickButtonEvent(OnClick_Retry);
        Button_Next.BindOnClickButtonEvent(OnClick_Next);
    }

    public void SetStageResult(JudgmentGrade judgmentGrade, int targetTimeMs, int inputTimeMs, int timingErrorMs)
    {
        Text_Grade.text = judgmentGrade.ToString();
        Text_TargetTime.text = GetSecondsDisplayText(targetTimeMs);
        Text_InputTime.text = GetInputTimeDisplayText(inputTimeMs);
        Text_TimingError.text = GetTimingErrorDisplayText(timingErrorMs);
    }

    private string GetSecondsDisplayText(int timeMs)
    {
        double timeSeconds = (double)timeMs / GameUtil.MillisecondsPerSecond;
        return timeSeconds.ToString(SecondsTimeFormat, CultureInfo.InvariantCulture);
    }

    private string GetInputTimeDisplayText(int inputTimeMs)
    {
        if (inputTimeMs == StageManager.NoInputTimeMs)
        {
            return NoInputTimeText;
        }

        return GetSecondsDisplayText(inputTimeMs);
    }

    private string GetTimingErrorDisplayText(int timingErrorMs)
    {
        if (timingErrorMs == StageManager.NoInputTimingErrorMs)
        {
            return NoInputTimingErrorText;
        }

        return timingErrorMs.ToString(CultureInfo.InvariantCulture) + TimingErrorUnitText;
    }

    private void OnClick_Retry()
    {
        GameManager.Instance.RequestRetryStage();
    }

    private void OnClick_Next()
    {
        GameManager.Instance.RequestNextStage();
    }
}