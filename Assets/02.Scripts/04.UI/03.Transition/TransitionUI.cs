//화면 전환 때 찜기 김으로 화면을 덮었다가 걷는 UI. 김 조각을 격자로 만들어 두고 덮기, 유지, 걷기를 맡는다
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TransitionUI : UIBase
{
    //시트 하나의 6장을 넣는 칸. 배열 안의 배열은 인스펙터에 보이지 않아서 한 번 감싼다
    [System.Serializable]
    private class SteamClumpSpriteSet
    {
        public Sprite[] Sprites;
    }

    //격자 배치. 0.2처럼 소수로 적은 거리는 "화면 크기의 몇 %인가"를 뜻함. 화면 비율이 달라도 같은 모양이 나오게 하기 위함이다
    private const int ColumnCount = 6;                  //화면 가로를 6칸으로 나눈다. 한 줄에는 조각을 7개 놓아 양쪽 끝 틈을 없앤다
    private const int VisibleRowCount = 5;              //화면 안에 보이는 줄 수
    private const int LatticeRowCount = 9;              //전체 줄 수. 화면 안 5줄 + 화면 아래에서 기다리는 4줄
    private const float ColumnStartOffset = 0.25f;       //첫 조각을 왼쪽으로 1/4 당겨 화면 왼쪽 끝에 틈이 생기지 않게 한다
    private const float RowStaggerOffset = 0.5f;        //벽돌 쌓듯 위에서 2, 4, 6, 8번째 줄을 오른쪽으로 반 칸 밀어 위아래 조각이 엇갈리게 한다
    private const float RowSpacingRatio = 0.2f;         //줄 사이 간격. 화면 높이의 20%
    private const float RowStartOffset = 0.6f;          //첫 줄을 화면 위 끝에서 줄 간격의 0.6배만큼 내려 놓는다
    private const float PositionJitterXRatio = 0.03f;   //격자 위치를 좌우로 최대 화면 너비의 3%만큼 흔들어 줄이 반듯해 보이지 않게 한다
    private const float PositionJitterYRatio = 0.04f;   //격자 위치를 위아래로 최대 화면 높이의 4%만큼 흔든다
    private const float FlipChance = 0.5f;              //조각을 좌우로 뒤집을 확률 50%

    //덮기
    private const float CoverDuration = 0.45f;          //조각 하나가 올라오는 시간(초)
    private const float CoverRowDelay = 0.14f;          //맨 아래 줄은 바로 출발하고 위 줄일수록 늦게 출발한다. 늦추는 시간의 최대값(초)
    private const float CoverRandomDelay = 0.06f;       //조각마다 0 ~ 0.06초를 더 늦춰 한꺼번에 움직이지 않게 한다
    private const float CoverOvershoot = 1.4f;          //제자리를 살짝 지나쳤다 돌아오는 정도. 클수록 많이 튄다
    private const float StartJitterXRatio = 0.06f;      //올라오기 시작하는 위치를 좌우로 최대 화면 너비의 6%만큼 흔든다
    private const float BaseFadeStartRatio = 0.5f;      //올라오는 시간의 절반(50%)이 지나면 바닥 김이 짙어지기 시작한다

    //유지
    private const float DriftSpeedRatio = 0.05f;        //덮인 동안 1초에 화면 높이의 5%씩 위로 흐른다
    private const float WrapMarginRatio = 0.36f;        //조각이 화면 위 끝보다 화면 높이의 36%만큼 더 올라가 완전히 안 보이면 아래로 돌려보낸다

    //걷기
    private const float UncoverDuration = 0.55f;        //조각 하나가 빠져나가는 시간(초)
    private const float UncoverCenterDelay = 0.1f;      //가장자리 조각은 바로, 가운데에 가까울수록 늦게 출발. 늦추는 시간의 최대값(초)
    private const float UncoverPushRatio = 0.7f;        //가운데에서 바깥 방향으로 화면너비의 70%만큼 밀려난다
    private const float UncoverRiseRatio = 0.3f;        //밀려나면서 위로 화면 높이의 30%만큼 더 떠오른다
    private const float BaseFadeOutDuration = 0.12f;    //바닥 김이 사라지는 시간(초)

    [SerializeField] private Image Image_SteamBase;
    [SerializeField] private RectTransform Rect_SteamClumpRoot;
    [SerializeField] private UISpriteImageSequencer Prefab_SteamClump;
    [SerializeField] private SteamClumpSpriteSet[] SpriteSet_SteamClumps;

    private UISpriteImageSequencer[] _steamClumps;
    private Sprite[][] _rotatedSpriteArrays; //시작 장면만 다르게 돌린 배열. 조각들이 같은 박자로 커졌다 작아지지 않게 한다
    private Sequence _currentSequence;
    private bool _isHolding; //덮인 채로 김이 흐르는 중

    private void Awake()
    {
        CreateRotatedSpriteArrays();
        CreateSteamClumps();
    }

    private void Update()
    {
        if (_isHolding == false)
        {
            return;
        }

        DriftSteamClumps(Time.unscaledDeltaTime);
    }

    //닫히면 진행 중인 연출을 멈춘다. 연출을 기다리던 쪽은 멈춘 시점에 기다림이 끝난다
    private void OnDisable()
    {
        KillCurrentSequence();
        _isHolding = false;
    }

    //김이 화면 아래에서 올라와 화면을 덮는다. 다 덮이면 끝난다
    public async UniTask CoverAsync()
    {
        KillCurrentSequence();
        _isHolding = false;

        Vector2 screenSize = Rect_SteamClumpRoot.rect.size;
        _currentSequence = DOTween.Sequence().SetUpdate(true);

        for (int i = 0; i < _steamClumps.Length; i++)
        {
            Vector2 targetPosition = ArrangeSteamClump(i);
            RectTransform clumpRect = (RectTransform)_steamClumps[i].transform;
            int row = i / (ColumnCount + 1);

            //화면 아래 대기줄은 보이지 않으므로 바로 제자리에 둔다
            if (row >= VisibleRowCount)
            {
                clumpRect.anchoredPosition = targetPosition;
                continue;
            }

            float startX = targetPosition.x + Random.Range(-StartJitterXRatio, StartJitterXRatio) * screenSize.x;
            float startY = -screenSize.y * 0.5f - clumpRect.rect.height;
            clumpRect.anchoredPosition = new Vector2(startX, startY);

            float delay = (VisibleRowCount - 1 - row) / (float)VisibleRowCount * CoverRowDelay + Random.Range(0f, CoverRandomDelay);
            _currentSequence.Insert(delay, clumpRect.DOAnchorPos(targetPosition, CoverDuration).SetEase(Ease.OutBack, CoverOvershoot));
        }

        SetSteamBaseAlpha(0f);
        float baseFadeStartTime = CoverDuration * BaseFadeStartRatio;
        _currentSequence.Insert(baseFadeStartTime, Image_SteamBase.DOFade(1f, CoverDuration - baseFadeStartTime));

        await _currentSequence.AsyncWaitForCompletion();
        _isHolding = true;
    }

    //애니메이션 없이 덮인 상태로 놓는다. 게임을 처음 켤 때처럼 보여 줄 이전 화면이 없을 때 쓴다
    public void CoverImmediately()
    {
        KillCurrentSequence();

        for (int i = 0; i < _steamClumps.Length; i++)
        {
            Vector2 targetPosition = ArrangeSteamClump(i);
            ((RectTransform)_steamClumps[i].transform).anchoredPosition = targetPosition;
        }

        SetSteamBaseAlpha(1f);
        _isHolding = true;
    }

    //가운데부터 열리며 김이 바깥으로 밀려나 빠진다. 다 빠지면 끝난다
    public async UniTask UncoverAsync()
    {
        KillCurrentSequence();
        _isHolding = false;

        Vector2 screenSize = Rect_SteamClumpRoot.rect.size;
        float maxDistance = screenSize.magnitude * 0.5f;
        _currentSequence = DOTween.Sequence().SetUpdate(true);
        _currentSequence.Insert(0f, Image_SteamBase.DOFade(0f, BaseFadeOutDuration));

        foreach (UISpriteImageSequencer steamClump in _steamClumps)
        {
            RectTransform clumpRect = (RectTransform)steamClump.transform;
            Vector2 position = clumpRect.anchoredPosition;
            Vector2 direction = position.normalized; //화면 가운데(0, 0)에서 조각을 향하는 방향
            Vector2 targetPosition = position + direction * screenSize.x * UncoverPushRatio + Vector2.up * screenSize.y * UncoverRiseRatio;
            float delay = Mathf.Max(0f, (1f - position.magnitude / maxDistance) * UncoverCenterDelay);

            _currentSequence.Insert(delay, clumpRect.DOAnchorPos(targetPosition, UncoverDuration).SetEase(Ease.InCubic));
        }

        await _currentSequence.AsyncWaitForCompletion();
    }

    //시트마다 6가지 시작 장면으로 돌린 배열을 한 번만 만들어둔다
    private void CreateRotatedSpriteArrays()
    {
        int frameCount = SpriteSet_SteamClumps[0].Sprites.Length;
        _rotatedSpriteArrays = new Sprite[SpriteSet_SteamClumps.Length * frameCount][];

        for (int setIndex = 0; setIndex < SpriteSet_SteamClumps.Length; setIndex++)
        {
            Sprite[] sprites = SpriteSet_SteamClumps[setIndex].Sprites;

            for (int startIndex = 0; startIndex < frameCount; startIndex++)
            {
                Sprite[] rotatedSprites = new Sprite[frameCount];

                for (int i = 0; i < frameCount; i++)
                {
                    rotatedSprites[i] = sprites[(startIndex + i) % frameCount];
                }

                _rotatedSpriteArrays[setIndex * frameCount + startIndex] = rotatedSprites;
            }
        }
    }

    //조각을 줄 순서대로 만든다. 나중에 만든 아래 줄이 앞에 그려진다
    private void CreateSteamClumps()
    {
        int clumpCount = LatticeRowCount * (ColumnCount + 1);
        _steamClumps = new UISpriteImageSequencer[clumpCount];

        for (int i = 0; i < clumpCount; i++)
        {
            _steamClumps[i] = Instantiate(Prefab_SteamClump, Rect_SteamClumpRoot);
        }
    }

    //조각의 모양, 시작 장면, 좌우 뒤집기를 무작위로 정하고 격자 위치를 돌려준다. 덮을 때마다 배치가 조금씩 달라진다
    private Vector2 ArrangeSteamClump(int clumpIndex)
    {
        UISpriteImageSequencer steamClump = _steamClumps[clumpIndex];
        int arrayIndex = Random.Range(0, _rotatedSpriteArrays.Length);
        steamClump.PlaySequence(_rotatedSpriteArrays[arrayIndex], SequenceEndMode.Loop);

        float flipX = Random.value < FlipChance ? -1f : 1f;
        steamClump.transform.localScale = new Vector3(flipX, 1f, 1f);

        return CalculateLatticePosition(clumpIndex);
    }

    //격자 칸의 위치(화면 가운데 기준)에 흔들림을 더해 돌려준다. 홀수 줄은 옆으로 밀어 벽돌처럼 엇갈리게 놓는다
    private Vector2 CalculateLatticePosition(int clumpIndex)
    {
        Vector2 screenSize = Rect_SteamClumpRoot.rect.size;
        int row = clumpIndex / (ColumnCount + 1);
        int column = clumpIndex % (ColumnCount + 1);
        float rowOffset = (row % 2) * RowStaggerOffset;

        float xFromLeft = (column - ColumnStartOffset + rowOffset) / ColumnCount * screenSize.x;
        float yFromTop = (row + RowStartOffset) * RowSpacingRatio * screenSize.y;
        xFromLeft += Random.Range(-PositionJitterXRatio, PositionJitterXRatio) * screenSize.x;
        yFromTop += Random.Range(-PositionJitterYRatio, PositionJitterYRatio) * screenSize.y;

        return new Vector2(xFromLeft - screenSize.x * 0.5f, screenSize.y * 0.5f - yFromTop);
    }

    //유지 구간에서 김이 천천히 올라간다. 화면 위로 완전히 빠진 조각은 화면 밖 아래로 돌아가 격자 간격을 유지한다
    private void DriftSteamClumps(float deltaTime)
    {
        float screenHeight = Rect_SteamClumpRoot.rect.height;
        float riseDistance = screenHeight * DriftSpeedRatio * deltaTime;
        float wrapY = screenHeight * (0.5f + WrapMarginRatio);
        float latticeHeight = LatticeRowCount * RowSpacingRatio * screenHeight;

        foreach (UISpriteImageSequencer steamClump in _steamClumps)
        {
            RectTransform clumpRect = (RectTransform)steamClump.transform;
            Vector2 position = clumpRect.anchoredPosition;
            position.y += riseDistance;

            if (position.y > wrapY)
            {
                position.y -= latticeHeight;
                clumpRect.SetAsLastSibling(); //아래로 돌아간 조각이 앞에 그려지게 한다
            }

            clumpRect.anchoredPosition = position;
        }
    }

    private void SetSteamBaseAlpha(float alpha)
    {
        Color baseColor = Image_SteamBase.color;
        baseColor.a = alpha;
        Image_SteamBase.color = baseColor;
    }

    private void KillCurrentSequence()
    {
        if (_currentSequence == null)
        {
            return;
        }

        _currentSequence.Kill();
        _currentSequence = null;
    }
}
