//가짜 숫자 방해요소 하나의 설정. FakeNumberSabotageData.json에 대응. Id는 SabotageData와 같다
using System;

[Serializable]
public class FakeNumberSabotageData : GameDataBase
{
    public int ShowDurationMs; //숫자 하나가 화면에 떠 있는 시간
}
