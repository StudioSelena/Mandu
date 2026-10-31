//스테이지 선택창 목록의 줄 하나. 스테이지 번호를 보여주고 눌리면 만든 쪽에 스테이지 Id를 알린다
using UnityEngine;
using TMPro;
using System;

public class StageSlotUI : MonoBehaviour
{
    [SerializeField] private UIButton Button_Stage;
    [SerializeField] private TMP_Text Text_Label;

    private string _stageId;
    private string _stageNumberText;
    private Action<string> _onClickStageSlot; //눌렀을 때 부를 메서드. 줄을 만든 StageSelectUI가 넘겨준다

    public string StageNumberText
    {
        get { return _stageNumberText; }
    }

    private void OnEnable()
    {
        Button_Stage.BindOnClickButtonEvent(OnClick_Stage);
    }

    public void InitStageSlot(string stageId, string stageNumberText, Action<string> onClickStageSlot)
    {
        _stageId = stageId;
        _stageNumberText = stageNumberText;
        _onClickStageSlot = onClickStageSlot;

        Text_Label.text = stageNumberText;
    }

    public void SetSelected(bool isSelected)
    {
        Button_Stage.SetSelected(isSelected);
    }

    private void OnClick_Stage()
    {
        _onClickStageSlot?.Invoke(_stageId);
    }
}
