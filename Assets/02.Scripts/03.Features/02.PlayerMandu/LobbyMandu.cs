//로비 왼쪽 만두. 로비가 열려있는 동안 대기 동작을 한다
using UnityEngine;

public class LobbyMandu : MonoBehaviour
{
    [SerializeField] private PlayerManduAnimatorController AnimatorController_Mandu;

    //생성된 프레임에 자식 Animator가 먼저 켜진다는 보장이 없어 모든 OnEnable이 끝난 Start에서 바꾼다
    private void Start()
    {
        AnimatorController_Mandu.SetState(PlayerManduAnimState.Idle);
    }
}
