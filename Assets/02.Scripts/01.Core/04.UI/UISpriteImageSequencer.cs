// 스프라이트 배열을 순서대로 교체해 재생하는 UI 이미지 시퀀서 (timeScale 영향 없음)
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UISpriteImageSequencer : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private Sprite[] SpriteArray_Sprite;
    [SerializeField] private float _sequenceInterval = 0.1f;
    [SerializeField] private bool _isLoop = true;

    private Image _image;
    private CancellationTokenSource _cancelToken;

    private void Awake()
    {
        _image = this.GetComponent<Image>();
    }

    private void OnEnable()
    {
        PlayAnimation().Forget();
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    public async UniTaskVoid PlayAnimation()
    {
        if (SpriteArray_Sprite == null || SpriteArray_Sprite.Length == 0)
        {
            return;
        }

        StopAnimation();
        _cancelToken = new CancellationTokenSource();
        CancellationToken token = _cancelToken.Token;

        int currentIndex = 0;
        _image.sprite = SpriteArray_Sprite[currentIndex];

        // 다음 그림으로 넘어갈 예정 시각. 시각표처럼 간격만큼 더해 가므로 늦어진 시간이 쌓이지 않는다
        double nextFrameTime = Time.unscaledTimeAsDouble + _sequenceInterval;

        while (true)
        {
            bool isCanceled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();
            if (isCanceled)
            {
                return;
            }

            if (Time.unscaledTimeAsDouble < nextFrameTime)
            {
                continue;
            }

            // 한 번 갱신에 한 장만 넘긴다. 렉이 걸려도 모든 그림이 나오게 하기 위해서다
            nextFrameTime += _sequenceInterval;
            currentIndex++;

            if (currentIndex >= SpriteArray_Sprite.Length)
            {
                if (_isLoop)
                {
                    currentIndex = 0;
                }
                else
                {
                    break;
                }
            }

            _image.sprite = SpriteArray_Sprite[currentIndex];
        }
    }

    public void StopAnimation()
    {
        if (_cancelToken != null)
        {
            _cancelToken.Cancel();
            _cancelToken.Dispose();
            _cancelToken = null;
        }
    }

    private void OnDestroy()
    {
        StopAnimation();
    }
}