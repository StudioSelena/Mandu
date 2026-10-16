//결과 화면. 등급, 기록, 다시하기와 다음 버튼 등을 맡는 UI
using System.Globalization;
using TMPro;
using UnityEngine;

public class ResultUI : UIBase
{
    private const string TimingErrorUnitText = "MS";
    private const string NoInputTimingErrorText = "-";

    [SerializeField] private TMP_Text Text_Grade;
    [SerializeField] private TMP_Text Text_TargetTime;
    [SerializeField] private TMP_Text Text_InputTime;
    [SerializeField] private TMP_Text Text_TimingError;
    [SerializeField] private TMP_Text Text_TryCount;
    [SerializeField] private UIButton Button_Retry;
    [SerializeField] private UIButton Button_Next;

    private void OnEnable()
    {
        Button_Retry.BindOnClickButtonEvent(OnClick_Retry);
        Button_Next.BindOnClickButtonEvent(OnClick_Next);
    }

    public void SetStageResult(StagePlayResult stagePlayResult, int tryCount)
    {
        Text_Grade.text = stagePlayResult.JudgmentGrade.ToString();
        Text_TargetTime.text = GameUtil.GetSecondsDisplayText(stagePlayResult.TargetTimeMs);
        Text_InputTime.text = GameUtil.GetInputTimeDisplayText(stagePlayResult.InputTimeMs);
        Text_TimingError.text = GetTimingErrorDisplayText(stagePlayResult.TimingErrorMs);
        Text_TryCount.text = tryCount.ToString(CultureInfo.InvariantCulture);
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