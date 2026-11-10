//스테이지 하나의 누적 기록(시도 횟수, 최고 등급, 최고 오차, 해금)을 담는 Model. 세이브 슬롯 파일에 그대로 저장된다
using System;
using UnityEngine;

[Serializable]
public class StageRecordModel
{
    [SerializeField] private string _stageId;
    [SerializeField] private int _tryCount;
    [SerializeField] private JudgmentGrade _bestGrade = JudgmentGrade.None;
    [SerializeField] private int _bestTimingErrorMs = StageManager.NoInputTimingErrorMs; //기록이 없으면 입력 없음과 같은 값
    [SerializeField] private bool _isUnlocked;          //앞 스테이지가 해금등급 이상이 된 순간 true. 그 뒤로 안 바뀜
    [SerializeField] private bool _isUnlockDisplayed;   //스테이지 선택창에서 반짝 공개를 보여줬는가

    //Id는 만들 때 한 번만 정한다. 딕셔너리 키도 이 값에서 꺼내므로 둘이 어긋나지 않는다
    public StageRecordModel(string stageId)
    {
        _stageId = stageId;
    }

    public string StageId
    {
        get { return _stageId; }
    }

    public int TryCount
    {
        get { return _tryCount; }
    }

    public JudgmentGrade BestGrade
    {
        get { return _bestGrade; }
    }

    public int BestTimingErrorMs
    {
        get { return _bestTimingErrorMs; }
    }

    public bool IsUnlocked
    {
        get { return _isUnlocked; }
    }

    public bool IsUnlockDisplayed
    {
        get { return _isUnlockDisplayed; }
    }

    public void IncreaseTryCount()
    {
        _tryCount++;
    }

    //해금은 잠기는 쪽으로 되돌리지 않으므로 여는 메서드만 둔다
    public void Unlock()
    {
        _isUnlocked = true;
    }

    //최고 등급과 최고 오차는 이 메서드에서만 바꾼다. 등급이 높으면 둘 다 바꾸고, 같으면 오차가 작을 때만 바꾼다
    public void UpdateBestRecord(JudgmentGrade judgmentGrade, int timingErrorMs)
    {
        if (judgmentGrade > _bestGrade)
        {
            _bestGrade = judgmentGrade;
            _bestTimingErrorMs = timingErrorMs;
            return;
        }

        //Fail끼리는 입력없음(-1)이 섞여 있어 오차를 비교하지 않음
        if (judgmentGrade != _bestGrade || judgmentGrade == JudgmentGrade.Fail)
        {
            return;
        }

        if (timingErrorMs < _bestTimingErrorMs)
        {
            _bestTimingErrorMs = timingErrorMs;
        }
    }
}