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
    [SerializeField] private SequenceEndMode _sequenceEndMode = SequenceEndMode.Loop;
    [SerializeField] private bool _isMatchSpriteSize = false; //켜면 재생 시작 시 이미지 크기와 기준점을 스프라이트(ppu, pivot)에 맞춘다

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

    //실행 중 재생할 스프라이트와 끝 처리 방식을 바꾸고 처음부터 재생한다
    public void PlaySequence(Sprite[] sprites, SequenceEndMode sequenceEndMode)
    {
        SpriteArray_Sprite = sprites;
        _sequenceEndMode = sequenceEndMode;
        PlayAnimation().Forget();
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

        //모든 그림이 같은 셀 크기라 재생 시작 때 한 번만 맞춘다
        if (_isMatchSpriteSize == true)
        {
            MatchImageToSprite(SpriteArray_Sprite[currentIndex]);
        }

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
                switch (_sequenceEndMode)
                {
                    case SequenceEndMode.Loop:
                        currentIndex = 0;
                        break;

                    case SequenceEndMode.ReturnToFirstFrame:
                        _image.sprite = SpriteArray_Sprite[0];
                        return;

                    default:
                        return;
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

    //Image 크기를 스프라이트 원래 크기(ppu 반영)로, 기준점을 스프라이트 pivot으로 맞춘다
    private void MatchImageToSprite(Sprite sprite)
    {
        _image.SetNativeSize();
        _image.rectTransform.pivot = sprite.pivot / sprite.rect.size;
    }

    private void OnDestroy()
    {
        StopAnimation();
    }
}