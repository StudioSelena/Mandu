//결과 화면. 등급, 기록, 다시하기와 다음 버튼 등을 맡는 UI
using Cysharp.Threading.Tasks;
using System.Globalization;
using TMPro;
using UnityEngine;

public class ResultUI : UIBase
{
    private const int DefaultGradeFrameStep = 1;        //모든 장을 보여준다
    private const int GoodAndNiceGradeFrameStep = 2;    //2장마다 1장을 보여준다
    private const int PerfectGradeFrameStep = 4;        //4장마다 1장을 보여준다

    [SerializeField] private UISpriteImageSequencer ImageSequencer_Grade;
    [SerializeField] private TMP_Text Text_TargetTime;
    [SerializeField] private TMP_Text Text_InputTime;
    [SerializeField] private TMP_Text Text_TimingError;
    [SerializeField] private TMP_Text Text_TryCount;
    [SerializeField] private UIButton Button_Exit;
    [SerializeField] private UIButton Button_Retry;
    [SerializeField] private UIButton Button_Next;

    private string _loadedGradeAddress; //지금 빌려 와서 보여주는 등급 이미지 주소. 화면 닫을 때 반납한다

    private void OnEnable()
    {
        Button_Exit.BindOnClickButtonEvent(OnClick_Exit);
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
        Text_TimingError.text = TimeUtil.GetTimingErrorDisplayText(stagePlayResult.TimingErrorMs);
        Text_TryCount.text = tryCount.ToString(CultureInfo.InvariantCulture);
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

        Sprite[] pickedSprites = PickGradeSprites(gradeSprites, GetGradeFrameStep(judgmentGrade));
        ImageSequencer_Grade.PlaySequence(pickedSprites, GetGradeSequenceEndMode(judgmentGrade));
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

    //등급마다 몇 장에 1장을 보여줄지 정한다. 장을 솎으면 모니터 주사율과 상관없이 재생 시간이 줄어든다
    private int GetGradeFrameStep(JudgmentGrade judgmentGrade)
    {
        switch (judgmentGrade)
        {
            case JudgmentGrade.Good:
            case JudgmentGrade.Nice:
                return GoodAndNiceGradeFrameStep;

            case JudgmentGrade.Perfect:
                return PerfectGradeFrameStep;

            default:
                return DefaultGradeFrameStep;
        }
    }

    //frameStep장마다 1장을 골라 새 배열로 만든다. 끝 모습이 바뀌지 않도록 마지막 장은 항상 넣는다
    private Sprite[] PickGradeSprites(Sprite[] gradeSprites, int frameStep)
    {
        if (frameStep <= DefaultGradeFrameStep)
        {
            return gradeSprites;
        }

        int lastIndex = gradeSprites.Length - 1;
        int pickedCount = (lastIndex + frameStep - 1) / frameStep + 1; //0번부터 frameStep씩 건너뛴 장 수에 마지막 장 자리 하나를 더한 수
        Sprite[] pickedSprites = new Sprite[pickedCount];

        for (int i = 0; i < pickedCount; i++)
        {
            //마지막 칸은 계산한 번호가 배열을 넘을 수 있어 마지막 장 번호로 맞춘다
            pickedSprites[i] = gradeSprites[Mathf.Min(i * frameStep, lastIndex)];
        }

        return pickedSprites;
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

    private void OnClick_Exit()
    {
        GameManager.Instance.RequestExitGamePlay();
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