//입력 액션을 켜고 끄며, 빼기 입력 시간을 이벤트로 알리는 매니저
using UnityEngine;
using UnityEngine.InputSystem;

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
        _gameInputActions.Stage.Pull.performed += HandlePullPerformed;
    }

    public void EnableStageInput()
    {
        _gameInputActions.Stage.Enable();
    }

    public void DisableStageInput()
    {
        _gameInputActions.Stage.Disable();
    }

    private void HandlePullPerformed(InputAction.CallbackContext context)
    {
        PullInputEvent?.Invoke(context.time);
    }

    private void OnDestroy()
    {
        if (_gameInputActions == null)
        {
            return;
        }

        _gameInputActions.Stage.Pull.performed -= HandlePullPerformed;
        _gameInputActions.Dispose();
    }
}