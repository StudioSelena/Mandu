//스테이지 선택창 목록의 줄 하나. 스테이지 번호와 잠김을 보여주고 눌리면 만든 쪽에 스테이지 Id를 알린다
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StageSlotUI : MonoBehaviour
{
    [SerializeField] private UIButton Button_Stage;
    [SerializeField] private TMP_Text Text_Label;
    [SerializeField] private Image Image_Locked;    //잠긴 줄 전체를 어둡게 덮는 판

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

    //잠긴 줄은 덮개로 어둡게 하고 누를 수 없게 한다. 누를 수 없으니 고를수도 없다
    public void SetStageUnlocked(bool isUnlocked)
    {
        Image_Locked.gameObject.SetActive(isUnlocked == false);
        Button_Stage.SetInteractable(isUnlocked);
    }

    private void OnClick_Stage()
    {
        _onClickStageSlot?.Invoke(_stageId);
    }
}
