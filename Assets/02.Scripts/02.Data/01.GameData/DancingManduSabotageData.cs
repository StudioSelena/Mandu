//춤추는 만두 방해요소 하나의 설정. DancingManduSabotageData.json에 대응. Id는 SabotageData와 같다
using System;

[Serializable]
public class DancingManduSabotageData : GameDataBase
{
    public int DanceDurationMs; //만두 한 마리가 나타나서 사라지기까지의 시간
    public int LoopDurationMs; //박자 한 묶음의 길이. 이 간격으로 묶음을 반복한다
}