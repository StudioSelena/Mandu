//춤추는 만두 방해요소. 삶기 시작 뒤 박자에 맞춰 화면 여기저기에 만두를 잠깐 띄워 플레이어가 박자를 따라 세게 만든다
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class DancingManduSabotage : MonoBehaviour, ISabotage
{
    private const int NoEmptySlotIndex = -1; //빈 자리를 찾지 못했을 때 돌려주는 번호
    private const int MaxSpawnPositionTryCount = 10; //가운데에 걸리면 다시 뽑는 최대 횟수

    [SerializeField] private Transform[] Slot_DancingMandus; //풀 오브젝트 자리. 켜고 끄고 옮기는 대상
    [SerializeField] private Vector2 _spawnAreaHalfSize = new Vector2(7.3f, 4.3f); //화면 가운데 기준 등장범위의 반쪽 크기. 스팀덱 화면 맞춤
    [SerializeField] private Vector2 _excludedAreaHalfSize = new Vector2(2f, 2f); //메인만두를 피하는 가운데 범위의 반쪽

    private IReadOnlyList<DancingManduBeatData> _beatDataList; //매 프레임 확인하므로 준비할 때 한 번 받아둔다
    private int _loopDurationMs;
    private int _nextBeatIndex; //지금 묶음 안에서 다음 나올 박자 순번
    private int _loopCount; //지금까지 끝낸 묶음 수
    private int[] _slotHideTimeMs; //자리별로 만두를 숨길 시각. 삶기 시작부터 잰 밀리초. Slot_DancingMandus와 같은 번호끼리 짝
    private Dictionary<string, AudioClip> _beatSoundClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        _slotHideTimeMs = new int[Slot_DancingMandus.Length];

        //삶기 시작 전에는 Update를 부르지 않는다
        enabled = false;
    }

    //빌려둔 박자소리를 반납한다. 사보타지매니저가 이 방해요소를 닫아 오브젝트가 없어질 때 반복한다
    private void OnDestory()
    {
        if (ResourceManager.Instance != null)
        {
            foreach (string soundAddress in _beatSoundClips.Keys)
            {
                ResourceManager.Instance.Release(soundAddress);
            }
        }

        _beatSoundClips.Clear();
    }

    public void PrepareSabotage(string sabotageId)
    {
        _beatDataList = GameDataManager.Instance.GetDancingManduBeatDataList(sabotageId);

        DancingManduSabotageData sabotageData = GameDataManager.Instance.GetDancingManduSabotageData(sabotageId);
        if (sabotageData != null)
        {
            _loopDurationMs = sabotageData.LoopDurationMs;
        }

        HideAllDancingMandus();

        LoadBeatSounds().Forget(); //준비할 때 소리 빌림
    }

    public void StartSabotage()
    {
        _nextBeatIndex = 0;
        _loopCount = 0;

        enabled = true; //켜져 있는 동안만 Update가 불린다
    }

    public void StopSabotage()
    {
        enabled = false;
        HideAllDancingMandus();
    }

    private void Update()
    {
        double elapsedSeconds = StageManager.Instance.GetBoilingElapsedSeconds();
        int elapsedMs = (int)(elapsedSeconds * TimeUtil.MillisecondsPerSecond);

        HideExpiredDancingMandus(elapsedMs);
        ShowNextDancingManduIfReached(elapsedMs);
    }

    private void HideAllDancingMandus()
    {
        foreach (Transform slot in Slot_DancingMandus)
        {
            slot.gameObject.SetActive(false);
        }
    }

    //춤 시간이 다 된 만두를 숨김
    private void HideExpiredDancingMandus(int elapsedMs)
    {
        for (int i = 0; i < Slot_DancingMandus.Length; i++)
        {
            GameObject slotObject = Slot_DancingMandus[i].gameObject;
            if (slotObject.activeSelf == false)
            {
                continue;
            }

            if (elapsedMs < _slotHideTimeMs[i])
            {
                continue;
            }

            slotObject.SetActive(false);
        }
    }

    //다음 박자 시각이 지났으면 만두를 띄우고 다음 박자로 넘어간다. 밀린 박자는 건너뛴다
    private void ShowNextDancingManduIfReached(int elapsedMs)
    {
        if (_beatDataList == null || _beatDataList.Count == 0)
        {
            return;
        }

        if (elapsedMs < CalculateCurrentBeatTimeMs())
        {
            return;
        }

        //일시정지나 창 전환으로 여러 박자가 밀렸으면 지난 박자는 건너뛰고 가장 최근 박자만 출력
        while (CalculateNextBeatTimeMs() <= elapsedMs)
        {
            AdvanceToNextBeat();
        }

        int beatTimeMs = CalculateCurrentBeatTimeMs();
        PlayBeatSound(_beatDataList[_nextBeatIndex].SoundAddress);
        ShowDancingManduAtRandomPosition(beatTimeMs, CalculateNextBeatTimeMs());

        AdvanceToNextBeat();
    }

    //다음에 올 박자의 시각. 묶음의 마지막 박자면 다음 묶음의 첫 박자다. 만두를 이 시각에 숨겨 화면 변화가 박자에만 생기게 한다
    private int CalculateNextBeatTimeMs()
    {
        int nextBeatIndex = _nextBeatIndex + 1;
        if (nextBeatIndex < _beatDataList.Count)
        {
            return _loopCount * _loopDurationMs + _beatDataList[nextBeatIndex].BeatTimeMs;
        }

        return (_loopCount + 1) * _loopDurationMs + _beatDataList[0].BeatTimeMs;
    }

    //지금 차례인 박자의 시각. 묶음 수만큼 반복 간격을 더한다
    private int CalculateCurrentBeatTimeMs()
    {
        return _loopCount * _loopDurationMs + _beatDataList[_nextBeatIndex].BeatTimeMs;
    }

    //다음 박자로 넘어간다. 묶음의 마지막 박자였으면 처음 박자로 돌아가서 묶음 수를 센다
    private void AdvanceToNextBeat()
    {
        _nextBeatIndex++;
        if (_nextBeatIndex >= _beatDataList.Count)
        {
            _nextBeatIndex = 0;
            _loopCount++;
        }
    }

    //빈자리 하나를 랜덤 위치로 옮겨 켜고 다음 박자에 숨긴다. 빈 자리가 없으면 그 박자는 건너뛴다
    private void ShowDancingManduAtRandomPosition(int beatTimeMs, int hideTimeMs)
    {
        int emptySlotIndex = FindEmptySlotIndex();
        if (emptySlotIndex == NoEmptySlotIndex)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[DancingManduSabotage] 빈 자리가 없어 {beatTimeMs}ms 박자를 건너뜁니다. 풀 크기와 춤 시간을 확인해 주세요.");
#endif
            return;
        }

        Transform slot = Slot_DancingMandus[emptySlotIndex];
        slot.localPosition = PickRandomSpawnPosition();
        _slotHideTimeMs[emptySlotIndex] = hideTimeMs;
        slot.gameObject.SetActive(true);
    }

    private int FindEmptySlotIndex()
    {
        for (int i = 0; i < Slot_DancingMandus.Length; i++)
        {
            if (Slot_DancingMandus[i].gameObject.activeSelf == false)
            {
                return i;
            }
        }
        
        return NoEmptySlotIndex;
    }

    //등장범위 안에서 가운데 제외 범위를 피한 자리를 뽑는다. 계속 가운데에 걸리면 마지막에 뽑은 자리를 쓴다
    private Vector3 PickRandomSpawnPosition()
    {
        Vector3 position = Vector3.zero;

        for (int tryCount = 0; tryCount < MaxSpawnPositionTryCount; tryCount++)
        {
            position.x = Random.Range(-_spawnAreaHalfSize.x, _spawnAreaHalfSize.x);
            position.y = Random.Range(-_spawnAreaHalfSize.y, _spawnAreaHalfSize.y);

            bool isInExcludedArea = Mathf.Abs(position.x) < _excludedAreaHalfSize.x && Mathf.Abs(position.y) < _excludedAreaHalfSize.y;
            if (isInExcludedArea == false)
            {
                return position;
            }
        }

        return position;
    }

    //박자 표에 나오는 소리를 주소마다 한 번씩만 빌려둔다. 다시하기로 다시 준비해도 이미 빌린 주소는 건너뛴다
    private async UniTaskVoid LoadBeatSounds()
    {
        if (_beatDataList == null)
        {
            return;
        }

        foreach (DancingManduBeatData beatData in _beatDataList)
        {
            string soundAddress = beatData.SoundAddress;
            if (string.IsNullOrEmpty(soundAddress) || _beatSoundClips.ContainsKey(soundAddress))
            {
                continue;
            }

            //기다리는 사이 같은 주소를 또 빌리지 않도록 자리를 먼저 잡아둠
            _beatSoundClips.Add(soundAddress, null);
            AudioClip clip = await ResourceManager.Instance.LoadAsset<AudioClip>(soundAddress);

            //기다리는 사이 오브젝트가 닫혀 목록이 비었으면 반납이 이미 끝났으니 더 빌리지 않음
            if (_beatSoundClips.ContainsKey(soundAddress) == false)
            {
                return;
            }

            _beatSoundClips[soundAddress] = clip;
        }
    }

    //박자 소리를 낸다. 아직 불러오는 중이거나 주소가 비어 있으면 소리 없이 넘어간다
    private void PlayBeatSound(string soundAddress)
    {
        if (string.IsNullOrEmpty(soundAddress))
        {
            return;
        }

        if (_beatSoundClips.TryGetValue(soundAddress, out AudioClip clip) == false)
        {
            return;
        }

        SoundManager.Instance.PlaySFX(clip);
    }
}
