//게임을 끌지 묻는 확인 창. 예 > 게임 종료, 아니요 > 팝업창 닫기
using UnityEngine;

public class QuitGameConfirmUI : UIBase
{
    [SerializeField] private UIButton Button_Yes;
    [SerializeField] private UIButton Button_No;

    private void OnEnable()
    {
        Button_Yes.BindOnClickButtonEvent(OnClick_Yes);
        Button_No.BindOnClickButtonEvent(OnClick_No);
    }

    private void OnClick_Yes()
    {
        GameManager.Instance.RequestQuitGame();
    }

    private void OnClick_No()
    {
        UIManager.Instance.CloseUI(UIType.QuitGameConfirmUI);
    }
}
