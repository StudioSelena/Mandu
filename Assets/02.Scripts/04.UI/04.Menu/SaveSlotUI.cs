//세이브 슬롯 선택창의 카드 하나. 슬롯 요약을 보여주고 눌리면 만든 쪽에 슬롯 번호를 알린다
using System;
using TMPro;
using UnityEngine;

public class SaveSlotUI : MonoBehaviour
{
    private const string SlotNumberTextFormat = "SLOT {0}";
    private const int SlotNumberOffset = 1;     //화면에 보이는 슬롯 번호는 1부터 센다
    private const string EmptyRecordText = "-"; //해금 규칙이 생기기 전까지 진행 스테이지와 별을 비워둔다

    [SerializeField] private UIButton Button_Slot;
    [SerializeField] private TMP_Text Text_SlotNumber;
    [SerializeField] private GameObject Group_Record;       //기록이 있는 슬롯에서만 켠다
    [SerializeField] private GameObject Text_NewGame;      //빈 슬롯에만 켠다
    [SerializeField] private TMP_Text Text_ProgressValue;
    [SerializeField] private TMP_Text Text_StarsValue;
    [SerializeField] private TMP_Text Text_LastPlayedValue;

    private int _slotIndex;
    private Action<int> _onClickSaveSlot; //눌렸을 때 부를 메서드. 카드를 가진 SaveSlotSelectUI가 넘겨준다

    private void OnEnable()
    {
        Button_Slot.BindOnClickButtonEvent(OnClick_Slot);
    }

    public void InitSaveSlot(int slotIndex, Action<int> onClickSaveSlot)
    {
        _slotIndex = slotIndex;
        _onClickSaveSlot = onClickSaveSlot;

        Text_SlotNumber.text = string.Format(SlotNumberTextFormat, slotIndex + SlotNumberOffset);
    }

    //빈 슬롯은 새로 시작 문구만, 기록이 있으면 항목 세 줄을 보여준다
    public void SetSaveSlotSummary(SaveSlotSummary saveSlotSummary)
    {
        Group_Record.SetActive(saveSlotSummary.HasSaveFile);
        Text_NewGame.SetActive(saveSlotSummary.HasSaveFile == false);

        if (saveSlotSummary.HasSaveFile == false)
        {
            return;
        }

        Text_ProgressValue.text = EmptyRecordText;
        Text_StarsValue.text = EmptyRecordText;
        Text_LastPlayedValue.text = TimeUtil.GetDateDisplayText(saveSlotSummary.LastOpenedTicks);
    }

    private void OnClick_Slot()
    {
        _onClickSaveSlot?.Invoke(_slotIndex);
    }
}
