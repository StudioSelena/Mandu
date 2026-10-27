//클릭낚시 방해요소. 삶는 동안 누르지 말라는 버튼을 띄우고, 누르면 그 판을 실패로 만든다
using UnityEngine;

public class ClickBaitSabotage : UIBase, ISabotage
{
    [SerializeField] private UIButton Button_DontPush;

    private void OnEnable()
    {
        //꺼질 때 UIButton이 연결을 스스로 푼다
        Button_DontPush.BindOnClickButtonEvent(OnClick_DontPush);
    }

    //상세 표가 없어 준비할 것이 없다. 숨긴 채로 열리고 삶기 시작에 켜진다
    public void PrepareSabotage(string sabotageId)
    {

    }

    public void StartSabotage()
    {
        gameObject.SetActive(true);
    }

    public void StopSabotage()
    {
        gameObject.SetActive(false);
    }

    private void OnClick_DontPush()
    {
        StageManager.Instance.RequestFailStageByClickBait();
    }
}
