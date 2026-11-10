//방해요소 하나가 지켜야 할 약속. SabotageManager는 종류를 몰라도 이 세 가지만 부른다
using Cysharp.Threading.Tasks;

public interface ISabotage
{
    //스테이지 준비 상태에서 부른다. 데이터를 읽고 숨긴 채로 준비한다. 로딩할 것이 있으면 다 불러와야 끝난다
    UniTask PrepareSabotage(string sabotageId);

    //삶기 버튼을 누른 순간(진행중 진입) 부른다
    void StartSabotage();

    //판정이 끝나면 부른다. 멈추고 숨긴다
    void StopSabotage();
}