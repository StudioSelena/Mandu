//춤추는 만두가 나오는 박자 한 줄. DancingManduBeatData.json에 대응
using System;

[Serializable]
public class DancingManduBeatData : GameDataBase
{
    public string SabotageId; //어느 춤추는 만두 방해요소의 박자인지
    public int BeatTimeMs; //한 묶음 안에서의 시각. 반복 간격보다 작아야 한다
    public string SoundAddress;
}