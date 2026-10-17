//스테이지 한 판의 상태 전환과 판정을 맡는 매니저
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    public const int NoInputTimingErrorMs = -1;
    public const int NoInputTimeMs = -1;
    private const int FailGracePeriodMs = 5000;

    private StageState _currentStageState = StageState.None;
    private TargetTimeData _currentTargetTimeData;
    private double _boilStartTime;
    private double _timeLimitSeconds;
    private double _judgmentCompleteTime;

    [SerializeField] private int _resultDelayMs = 3000;

    public JudgmentGrade CurrentJudgmentGrade { get; private set; }
    public int CurrentTimingErrorMs { get; private set; }
    public int CurrentTargetTimeMs { get; private set; }
    public int CurrentInputTimeMs { get; private set; }

    public event System.Action<StageState> StageStateChangedEvent;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }
    }

    private void Start()
    {
        if (Instance != this)
        { 
            return; 
        }

        InputManager.Instance.PullInputEvent += HandlePullInput;
    }

    private void OnDestroy()
    {
        if (Instance != this)
        { 
            return; 
        }

        if (InputManager.Instance == null)
        { 
            return; 
        }

        InputManager.Instance.PullInputEvent -= HandlePullInput;
    }

    private void Update()
    {
        switch (_currentStageState)
        {
            case StageState.Boiling:
                CheckBoilingTimeLimit();
                break;

            case StageState.JudgmentComplete:
                CheckResultDelay();
                break;
        }
    }

    private void CheckBoilingTimeLimit()
    {
        double elapsedSeconds = Time.realtimeSinceStartupAsDouble - _boilStartTime;
        if (elapsedSeconds < _timeLimitSeconds)
        {
            return;
        }

        FailStageByTimeLimit();
    }

    private void CheckResultDelay()
    {
        double elapsedSeconds = Time.realtimeSinceStartupAsDouble - _judgmentCompleteTime;
        double resultDelaySeconds = (double)_resultDelayMs / TimeUtil.MillisecondsPerSecond;
        if (elapsedSeconds < resultDelaySeconds)
        {
            return;
        }

        ChangeStageState(StageState.ShowingResult);
    }

    public void PrepareStage(string stageId)
    {
        StageData stageData = GameDataManager.Instance.GetStageData(stageId);
        if (stageData == null)
        {
            return;
        }

        TargetTimeData targetTimeData = GameDataManager.Instance.GetTargetTimeData(stageData.TargetTimeId);
        if (targetTimeData == null)
        {
            return;
        }

        _currentTargetTimeData = targetTimeData;

        int timeLimitMs = _currentTargetTimeData.TargetTimeMs + _currentTargetTimeData.GoodWindowMs + FailGracePeriodMs;
        _timeLimitSeconds = (double)timeLimitMs / TimeUtil.MillisecondsPerSecond;

        _boilStartTime = 0;
        _judgmentCompleteTime = 0;
        CurrentJudgmentGrade = JudgmentGrade.None;
        CurrentTimingErrorMs = 0;
        CurrentTargetTimeMs = _currentTargetTimeData.TargetTimeMs;
        CurrentInputTimeMs = NoInputTimeMs;

        ChangeStageState(StageState.Ready);
    }

    public void RequestStartBoiling()
    {
        if (_currentStageState != StageState.Ready)
        {
            return;
        }

        _boilStartTime = Time.realtimeSinceStartupAsDouble;
        InputManager.Instance.EnableStageInput();

        ChangeStageState(StageState.Boiling);
    }

    private void HandlePullInput(double pullTime)
    {
        if (_currentStageState != StageState.Boiling)
        {
            return;
        }

        InputManager.Instance.DisableStageInput();
        double elapsedSeconds = pullTime - _boilStartTime;
        CurrentInputTimeMs = (int)(elapsedSeconds * TimeUtil.MillisecondsPerSecond);
        CurrentTimingErrorMs = GameUtil.CalculateTimingErrorMs(elapsedSeconds, _currentTargetTimeData.TargetTimeMs);
        CurrentJudgmentGrade = GameUtil.JudgeTimingGrade(CurrentTimingErrorMs, _currentTargetTimeData);

        _judgmentCompleteTime = Time.realtimeSinceStartupAsDouble;
        ChangeStageState(StageState.JudgmentComplete);
    }

    private void FailStageByTimeLimit()
    {
        InputManager.Instance.DisableStageInput();

        CurrentJudgmentGrade = JudgmentGrade.Fail;
        CurrentTimingErrorMs = NoInputTimingErrorMs;
        CurrentInputTimeMs = NoInputTimeMs;

        _judgmentCompleteTime = Time.realtimeSinceStartupAsDouble;
        ChangeStageState(StageState.JudgmentComplete);
    }

    private void ChangeStageState(StageState newStageState)
    {
        if (newStageState == _currentStageState)
        {
            return;
        }

        _currentStageState = newStageState;
        StageStateChangedEvent?.Invoke(newStageState);
    }
}