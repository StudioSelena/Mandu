//로비 화면. 이어하기, 싱글 플레이, 종료 같은 메인메뉴 버튼을 맡는 UI
using UnityEngine;

public class LobbyUI : UIBase
{
    [SerializeField] private UIButton Button_Continue;
    [SerializeField] private UIButton Button_SinglePlay;
    [SerializeField] private UIButton Button_Quit;

    //세이브가 하나도 없으면 이어하기는 보이지만 누를 수 없다. 플레이하고 돌아오면 바뀌므로 열 때마다 확인
    private void OnEnable()
    {
        Button_Continue.BindOnClickButtonEvent(OnClick_Continue);
        Button_SinglePlay.BindOnClickButtonEvent(OnClick_SinglePlay);
        Button_Quit.BindOnClickButtonEvent(OnClick_Quit);

        Button_Continue.SetInteractable(PlayRecordManager.Instance.HasAnySaveFile());
    }

    private void OnClick_Continue()
    {
        GameManager.Instance.RequestContinueGame();
    }

    private void OnClick_SinglePlay()
    {
        GameManager.Instance.RequestOpenSaveSlotSelect();
    }

    private void OnClick_Quit()
    {
        UIManager.Instance.OpenUI(UIRootType.Popup, UIType.QuitGameConfirmUI);
    }
}
