//목표 시간 하나와 등급별 허용 폭(밀리초). TargetTimeData.json에 대응
using System;

[Serializable]
public class TargetTimeData : GameDataBase
{
    public int TargetTimeMs;
    public int PerfectWindowMs;
    public int NiceWindowMs;
    public int GoodWindowMs;
}