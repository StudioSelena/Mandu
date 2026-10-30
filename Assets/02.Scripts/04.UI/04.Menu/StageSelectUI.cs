//스테이지 선택창. 스테이지를 골라 플레이에 들어가거나 로비로 돌아가는 UI
using UnityEngine;

public class StageSelectUI : UIBase
{
    [SerializeField] private UIButton Button_MainMenu;

    private void OnEnable()
    {
        Button_MainMenu.BindOnClickButtonEvent(OnClick_MainMenu);
    }

    private void OnClick_MainMenu()
    {
        GameManager.Instance.RequestOpenLobby();
    }
}
