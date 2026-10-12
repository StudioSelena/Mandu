//입력 액션을 켜고 끄며, 빼기 입력 시간을 이벤트로 알리는 매니저
using UnityEngine;

public class InputManager : SingletonBase<InputManager>
{
    private GameInputActions _gameInputActions;

    public event System.Action<double> PullInputEvent;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        _gameInputActions = new GameInputActions();
    }
}