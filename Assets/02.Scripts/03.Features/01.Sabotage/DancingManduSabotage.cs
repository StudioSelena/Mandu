//춤추는 만두 방해요소. 삶기 시작 뒤 박자에 맞춰 화면 여기저기에 만두를 잠깐 띄워 플레이어가 박자를 따라 세게 만든다
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
    private int _danceDurationMs;
    private int _loopDurationMs;
    private int _nextBeatIndex; //지금 묶음 안에서 다음 나올 박자 순번
    private int _loopCount; //지금까지 끝낸 묶음 수
    private int[] _slotHideTimeMs; //자리별로 만두를 숨길 시각. 삶기 시작부터 잰 밀리초. Slot_DancingMandus와 같은 번호끼리 짝

    private void Awake()
    {
        _slotHideTimeMs = new int[Slot_DancingMandus.Length];

        //삶기 시작 전에는 Update를 부르지 않는다
        enabled = false;
    }

    public void PrepareSabotage(string sabotageId)
    {
        _beatDataList = GameDataManager.Instance.GetDancingManduBeatDataList(sabotageId);

        DancingManduSabotageData sabotageData = GameDataManager.Instance.GetDancingManduSabotageData(sabotageId);
        if (sabotageData != null)
        {
            _danceDurationMs = sabotageData.DanceDurationMs;
            _loopDurationMs = sabotageData.LoopDurationMs;
        }

        HideAllDancingMandus();
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
            if (!slotObject.activeSelf)
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

    //다음 박자 시각이 지났으면 만두를 띄우고 다음 박자로 넘어간다. 한 묶음이 끝나면 처음 박자로 돌아가 반복한다
    private void ShowNextDancingManduIfReached(int elapsedMs)
    {
        if (_beatDataList == null || _beatDataList.Count == 0)
        {
            return;
        }

        int beatTimeMs = _loopCount * _loopDurationMs + _beatDataList[_nextBeatIndex].BeatTimeMs;
        if (elapsedMs < beatTimeMs)
        {
            return;
        }

        ShowDancingManduAtRandomPosition(beatTimeMs);

        _nextBeatIndex++;
        if (_nextBeatIndex >= _beatDataList.Count)
        {
            _nextBeatIndex = 0;
            _loopCount++;
        }
    }

    //빈자리 하나를 랜덤 위치로 옮겨 켠다. 빈 자리가 없으면 그 박자는 건너뛴다
    private void ShowDancingManduAtRandomPosition(int beatTimeMs)
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
        _slotHideTimeMs[emptySlotIndex] = beatTimeMs + _danceDurationMs;
        slot.gameObject.SetActive(true);
    }

    private int FindEmptySlotIndex()
    {
        for (int i = 0; i < Slot_DancingMandus.Length; i++)
        {
            if (!Slot_DancingMandus[i].gameObject.activeSelf)
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
            if (!isInExcludedArea)
            {
                return position;
            }
        }

        return position;
    }
}
