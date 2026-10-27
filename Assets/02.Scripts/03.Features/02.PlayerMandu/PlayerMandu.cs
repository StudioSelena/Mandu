//메인 만두 본체. 스테이지 상태와 판정 등급을 보고 어떤 애니메이션을 할지 정한다
using UnityEngine;

public class PlayerMandu : MonoBehaviour
{
    [SerializeField] private PlayerManduAnimatorController AnimatorController_Mandu;

    //생성된 프레임에 바로 첫 Ready가 오므로 Start가 아니라 OnEnable에서 구독
    private void OnEnable()
    {
        StageManager.Instance.StageStateChangedEvent += HandleStageStateChanged;
    }

    private void OnDisable()
    {
        if (StageManager.Instance == null)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent -= HandleStageStateChanged;
    }

    //ShowingResult는 판정 때 정한 동작을 그대로 이어가므로 다루지 않는다
    private void HandleStageStateChanged(StageState stageState)
    {
        switch (stageState)
        {
            case StageState.Ready:
                AnimatorController_Mandu.SetState(PlayerManduAnimState.EnterSteamer);
                break;
            case StageState.Boiling:
                AnimatorController_Mandu.SetState(PlayerManduAnimState.Boiling);
                break;
            case StageState.JudgmentComplete:
                AnimatorController_Mandu.SetState(GetResultAnimState());
                break;
        }
    }

    //판정이 끝난 순간 등급이 이미 정해져 있어 바로 성공과 실패를 나눈다
    private PlayerManduAnimState GetResultAnimState()
    {
        if (StageManager.Instance.CurrentJudgmentGrade == JudgmentGrade.Fail)
        {
            return PlayerManduAnimState.ResultFail;
        }

        return PlayerManduAnimState.ResultSuccess;
    }
}
