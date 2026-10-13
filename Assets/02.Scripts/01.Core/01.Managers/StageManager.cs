//스테이지 한 판의 상태 전환과 판정을 맡는 매니저
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    private const int FailGracePeriodMs = 5000;

    private StageState _currentStageState = StageState.None;
    private TargetTimeData _currentTargetTimeData;
    private double _boilStartTime;
    private double _timeLimitSeconds;

    public JudgmentGrade CurrentJudgmentGrade { get; private set; }
    public int CurrentTimingErrorMs { get; private set; }

    public event System.Action<StageState> StageStateChangedEvent;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }
    }
}