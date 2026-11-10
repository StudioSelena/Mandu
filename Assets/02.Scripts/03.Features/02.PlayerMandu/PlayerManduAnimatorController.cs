//메인 만두 애니메이터의 파라미터를 상태에 맞게 바꾸는 컴포넌트. 어떤 상태로 바꿀지는 PlayerMandu와 LobbyMandu가 정한다
using UnityEngine;

public class PlayerManduAnimatorController : MonoBehaviour
{
    private const string IsBoilingParameter = "IsBoiling";
    private const string IsResultSuccessParameter = "IsResultSuccess";
    private const string IsResultFailParameter = "IsResultFail";
    private const string IsIdleParameter = "IsIdle";

    [SerializeField] private Animator Animator_Mandu;

    private PlayerManduAnimState _currentAnimState = PlayerManduAnimState.None;

    //애니메이션을 바꾸는 유일한 진입점. 같은 상태로 다시 들어오면 무시한다
    public void SetState(PlayerManduAnimState newAnimState)
    {
        if (newAnimState == _currentAnimState)
        {
            return;
        }

        _currentAnimState = newAnimState;
        ResetAllAnimParameters();

        switch (_currentAnimState)
        {
            case PlayerManduAnimState.Boiling:
                Animator_Mandu.SetBool(IsBoilingParameter, true);
                break;

            case PlayerManduAnimState.ResultSuccess:
                Animator_Mandu.SetBool(IsResultSuccessParameter, true);
                break;

            case PlayerManduAnimState.ResultFail:
                Animator_Mandu.SetBool(IsResultFailParameter, true);
                break;

            case PlayerManduAnimState.Idle:
                Animator_Mandu.SetBool(IsIdleParameter, true);
                break;
        }
    }

    private void ResetAllAnimParameters()
    {
        Animator_Mandu.SetBool(IsBoilingParameter, false);
        Animator_Mandu.SetBool(IsResultSuccessParameter, false);
        Animator_Mandu.SetBool(IsResultFailParameter, false);
        Animator_Mandu.SetBool(IsIdleParameter, false);
    }
}
