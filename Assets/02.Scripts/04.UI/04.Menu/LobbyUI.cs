//로비 화면. 싱글 플레이와 종료 같은 메인메뉴 버튼을 맡는 UI
using UnityEngine;

public class LobbyUI : UIBase
{
    [SerializeField] private UIButton Button_SinglePlay;
    [SerializeField] private UIButton Button_Quit;

    private void OnEnable()
    {
        Button_SinglePlay.BindOnClickButtonEvent(OnClick_SinglePlay);
        Button_Quit.BindOnClickButtonEvent(OnClick_Quit);
    }

    private void OnClick_SinglePlay()
    {
        GameManager.Instance.RequestOpenStageSelect();
    }

    private void OnClick_Quit()
    {
        UIManager.Instance.OpenUI(UIRootType.Popup, UIType.QuitGameConfirmUI);
    }
}
