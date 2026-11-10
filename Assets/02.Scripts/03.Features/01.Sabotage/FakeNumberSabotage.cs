//가짜 숫자 방해요소. 삶기 시작 뒤 정해진 시각에 숫자를 잠깐 띄워 플레이어의 체감 시간을 흔든다
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FakeNumberSabotage : UIBase, ISabotage
{
    [SerializeField] private TMP_Text Text_FakeNumber;

    private IReadOnlyList<FakeNumberEventData> _eventDataList; //매 프레임 확인하므로 준비할 때 한 번 받아 둔다
    private int _showDurationMs;
    private int _nextEventIndex;
    private int _hideTimeMs; //지금 떠 있는 숫자를 숨길 시각. 삶기 시작부터 잰 밀리초

    public UniTask PrepareSabotage(string sabotageId)
    {
        _eventDataList = GameDataManager.Instance.GetFakeNumberEventDataList(sabotageId);

        FakeNumberSabotageData sabotageData = GameDataManager.Instance.GetFakeNumberSabotageData(sabotageId);
        if (sabotageData != null)
        {
            _showDurationMs = sabotageData.ShowDurationMs;
        }

        Text_FakeNumber.gameObject.SetActive(false);

        return UniTask.CompletedTask;
    }

    public void StartSabotage()
    {
        _nextEventIndex = 0;

        gameObject.SetActive(true); //켜져 있는 동안만 업데이트가 불린다
    }

    public void StopSabotage()
    {
        Text_FakeNumber.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        double elapsedSeconds = StageManager.Instance.GetBoilingElapsedSeconds();
        int elapsedMs = (int)(elapsedSeconds * TimeUtil.MillisecondsPerSecond);

        HideFakeNumberIfExpired(elapsedMs);
        ShowNextFakeNumberIfReached(elapsedMs);
    }

    //떠있는 숫자의 시간이 다 됐으면 숨긴다
    private void HideFakeNumberIfExpired(int elapsedMs)
    {
        if (Text_FakeNumber.gameObject.activeSelf == false)
        {
            return;
        }

        if (elapsedMs < _hideTimeMs)
        {
            return;
        }

        Text_FakeNumber.gameObject.SetActive(false);
    }

    //다음 숫자의 시각이 지났으면 띄우고 다음 순번으로 넘어간다
    private void ShowNextFakeNumberIfReached(int elapsedMs)
    {
        if (_eventDataList == null || _nextEventIndex >= _eventDataList.Count)
        {
            return;
        }

        FakeNumberEventData eventData = _eventDataList[_nextEventIndex];
        if (elapsedMs < eventData.ShowTimeMs)
        {
            return;
        }

        Text_FakeNumber.text = eventData.DisplayText;
        Text_FakeNumber.gameObject.SetActive(true);
        _hideTimeMs = eventData.ShowTimeMs + _showDurationMs;
        _nextEventIndex++;
    }
}