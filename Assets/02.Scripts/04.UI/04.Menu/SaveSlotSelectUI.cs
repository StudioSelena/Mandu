//세이브 슬롯 선택창. 카드 세 장 중 하나를 골라 스테이지 선택창으로 가거나 로비로 돌아가는 UI
using UnityEngine;

public class SaveSlotSelectUI : UIBase
{
    [SerializeField] private UIButton Button_MainMenu;
    [SerializeField] private SaveSlotUI[] SaveSlotUIs; //에디터에 배치한 카드. 배열 순서가 슬롯 번호

    //카드 수는 프리팹과 SaveSlotCount 두 곳에 있어 어긋나면 알린다
    private void Awake()
    {
#if UNITY_EDITOR
        if (SaveSlotUIs.Length != PlayRecordManager.SaveSlotCount)
        {
            Debug.LogWarning($"[SaveSlotSelectUI] 카드 수 {SaveSlotUIs.Length}와 슬롯 수 {PlayRecordManager.SaveSlotCount}가 다릅니다.");
        }
#endif

        for (int i = 0; i < SaveSlotUIs.Length; i++)
        {
            SaveSlotUIs[i].InitSaveSlot(i, OnClick_SaveSlot);
        }
    }

    //다른 슬롯을 플레이 후 돌아오면 날짜가 바뀌므로 열 때마다 요약을 다시 읽는다
    private void OnEnable()
    {
        Button_MainMenu.BindOnClickButtonEvent(OnClick_MainMenu);

        for (int i = 0; i < SaveSlotUIs.Length; i++)
        {
            SaveSlotUIs[i].SetSaveSlotSummary(PlayRecordManager.Instance.GetSaveSlotSummary(i));
        }
    }

    private void OnClick_SaveSlot(int slotIndex)
    {
        GameManager.Instance.RequestLoadSaveSlot(slotIndex);
    }

    private void OnClick_MainMenu()
    {
        GameManager.Instance.RequestOpenLobby();
    }
}
