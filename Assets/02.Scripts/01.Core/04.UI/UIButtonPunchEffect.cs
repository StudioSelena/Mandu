// 버튼을 누르는 순간 커졌다가 줄어드는 펀치 스케일 이펙트 (공용 컴포넌트, 아무 버튼에나 부착)
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonPunchEffect : MonoBehaviour, IPointerDownHandler
{
    private const float PunchScaleMultiplier = 1.075f;
    private const float ScaleUpDurationSeconds = 0.08f;
    private const float ScaleDownDurationSeconds = 0.12f;

    private Button _button;
    private RectTransform _rectTransform;
    private Vector3 _originalScale;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isPlayingPunchEffect = false;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _rectTransform = GetComponent<RectTransform>();
        _originalScale = _rectTransform.localScale;
    }

    private void OnDisable()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _isPlayingPunchEffect = false;

        if (_rectTransform != null)
        {
            _rectTransform.localScale = _originalScale;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (_button.IsInteractable() == false)
        {
            return;
        }

        PlayPunchEffect().Forget();
    }

    ///<summary> 재생 중인 펀치 이펙트가 끝날 때까지 기다린다. 재생 중이 아니면 즉시 끝난다 </summary>
    public async UniTask WaitPunchEffectEndAsync()
    {
        await UniTask.WaitWhile(() => _isPlayingPunchEffect);
    }

    private async UniTaskVoid PlayPunchEffect()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = _cancellationTokenSource.Token;

        _isPlayingPunchEffect = true;
        _rectTransform.localScale = _originalScale;

        bool isCanceled = await TweenScale(_originalScale, _originalScale * PunchScaleMultiplier, ScaleUpDurationSeconds, token);

        if (isCanceled)
        {
            return;
        }

        isCanceled = await TweenScale(_originalScale * PunchScaleMultiplier, _originalScale, ScaleDownDurationSeconds, token);

        if (isCanceled)
        {
            return;
        }

        _rectTransform.localScale = _originalScale;
        _isPlayingPunchEffect = false;
    }

    ///<summary> 정해진 시간 동안 크기를 일정한 속도로 바꾼다. 중간에 멈추면 true를 돌려준다 </summary>
    private async UniTask<bool> TweenScale(Vector3 fromScale, Vector3 toScale, float durationSeconds, CancellationToken token)
    {
        float elapsedSeconds = 0f;

        while (elapsedSeconds < durationSeconds)
        {
            if (token.IsCancellationRequested)
            {
                return true;
            }

            elapsedSeconds += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedSeconds / durationSeconds);

            _rectTransform.localScale = Vector3.Lerp(fromScale, toScale, progress);

            await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();
        }

        return token.IsCancellationRequested;
    }
}