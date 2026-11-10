//스테이지 하나의 설정. StageData.json에 대응
using System;

[Serializable]
public class StageData : GameDataBase
{
    public string TargetTimeId;
    public string SabotageId; //방해 요소가 없으면 null or 빈 문자열. 쓰는 쪽에서 string.IsNullOrEmpty로 확인
}