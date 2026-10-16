//스테이지 하나의 누적 기록(시도 횟수)을 담는 Model
using System;
using UnityEngine;

[Serializable]
public class StageRecordModel
{
    [SerializeField] private int _tryCount;

    public int TryCount
    {
        get { return _tryCount; }
    }

    public void IncreaseTryCount()
    {
        _tryCount++;
    }
}