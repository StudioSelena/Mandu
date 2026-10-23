//가짜 숫자를 띄우는 시각과 글자 한 줄. FakeNumberEventData.json에 대응
using System;

[Serializable]
public class FakeNumberEventData : GameDataBase
{
    public string SabotageId; //어느 가짜 숫자 방해요소의 목록인지
    public int ShowTimeMs; //삶기 버튼을 누른 순간부터 잰 시각
    public string DisplayText; //화면에 보일 글자. 형식을 자유롭게 바꾸려고 string
}
