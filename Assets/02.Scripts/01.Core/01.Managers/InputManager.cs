//입력 액션을 켜고 끄며, 빼기 입력 시간을 이벤트로 알리는 매니저
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : SingletonBase<InputManager>
{
    private GameInputActions _gameInputActions;

    public event System.Action<double> PullInputEvent;
#if UNITY_EDITOR
    public event System.Action ToggleDevModeEvent; //개발자 패널 여닫기 키. 출시 빌드에서는 빠진다
#endif

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        _gameInputActions = new GameInputActions();
        _gameInputActions.Stage.Pull.performed += HandlePullPerformed;

#if UNITY_EDITOR
        //개발자 키는 삶기 여부와 상관없이 늘 들어야 해서 Stage 맵과 따로 켠다
        _gameInputActions.Debug.ToggleDevMode.performed += HandleToggleDevModePerformed;
        _gameInputActions.Debug.Enable();
#endif
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void HandleToggleDevModePerformed(InputAction.CallbackContext context)
    {
        ToggleDevModeEvent?.Invoke();
    }
#endif

    private void OnDestroy()
    {
        if (_gameInputActions == null)
        {
            return;
        }

        _gameInputActions.Stage.Pull.performed -= HandlePullPerformed;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        _gameInputActions.Debug.ToggleDevMode.performed -= HandleToggleDevModePerformed;
        _gameInputActions.Dispose();
#endif
    }
}