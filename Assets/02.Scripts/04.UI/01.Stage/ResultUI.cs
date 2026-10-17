//결과 화면. 등급, 기록, 다시하기와 다음 버튼 등을 맡는 UI
using Cysharp.Threading.Tasks;
using System.Globalization;
using TMPro;
using UnityEngine;

public class ResultUI : UIBase
{
    private const string TimingErrorUnitText = "MS";
    private const string NoInputTimingErrorText = "-";

    [SerializeField] private UISpriteImageSequencer ImageSequencer_Grade;
    [SerializeField] private TMP_Text Text_TargetTime;
    [SerializeField] private TMP_Text Text_InputTime;
    [SerializeField] private TMP_Text Text_TimingError;
    [SerializeField] private TMP_Text Text_TryCount;
    [SerializeField] private UIButton Button_Retry;
    [SerializeField] private UIButton Button_Next;

    private string _loadedGradeAddress; //지금 빌려 와서 보여주는 등급 이미지 주소. 화면 닫을 때 반납한다

    private void OnEnable()
    {
        Button_Retry.BindOnClickButtonEvent(OnClick_Retry);
        Button_Next.BindOnClickButtonEvent(OnClick_Next);
    }

    private void OnDisable()
    {
        ReleaseGradeSprites();
    }

    public void SetStageResult(StagePlayResult stagePlayResult, int tryCount)
    {
        ShowGradeImage(stagePlayResult.JudgmentGrade).Forget();
        Text_TargetTime.text = TimeUtil.GetSecondsDisplayText(stagePlayResult.TargetTimeMs);
        Text_InputTime.text = TimeUtil.GetInputTimeDisplayText(stagePlayResult.InputTimeMs);
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

    private async UniTaskVoid ShowGradeImage(JudgmentGrade judgmentGrade)
    {
        ReleaseGradeSprites();

        if (judgmentGrade == JudgmentGrade.None)
        {
            return;
        }

        //빌려간 수는 LoadAsset을 부르는 순간 올라가므로 기다리기 전에 주소를 기록한다
        string address = GameUtil.GetJudgmentGradeAddress(judgmentGrade);
        _loadedGradeAddress = address;
        Sprite[] gradeSprites = await ResourceManager.Instance.LoadAsset<Sprite[]>(address);

        //기다리는 사이 화면이 닫혔거나 다른 등급으로 바뀐 경우
        if (gradeSprites == null || _loadedGradeAddress != address)
        {
            return;
        }

        ImageSequencer_Grade.PlaySequence(gradeSprites, GetGradeSequenceEndMode(judgmentGrade));
    }

    //Good, Nice는 마지막에 글자가 사라지는 연출이라 첫 그림으로 돌아가 등급을 계속 보여준다
    private SequenceEndMode GetGradeSequenceEndMode(JudgmentGrade judgmentGrade)
    {
        switch (judgmentGrade)
        {
            case JudgmentGrade.Good:
            case JudgmentGrade.Nice:
                return SequenceEndMode.ReturnToFirstFrame;

            default:
                return SequenceEndMode.HoldLastFrame;
        }
    }

    private void ReleaseGradeSprites()
    {
        if (string.IsNullOrEmpty(_loadedGradeAddress) == true)
        {
            return;
        }

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.Release(_loadedGradeAddress);
        }

        _loadedGradeAddress = null;
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