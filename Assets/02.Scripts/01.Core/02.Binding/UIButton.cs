// 버튼 클릭 이벤트를 코드로 연결하고 해제하는 공용 버튼 컴포넌트
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    [SerializeField] private Button Button_Base;
    [SerializeField] private Image Image_Base;
    [SerializeField] private Image Image_Selected;
    [SerializeField] private TMP_Text Text_Base;

    // true면 OnDisable에서 자동 해제하지 않고 외부에서 직접 해제
    private bool _isManualUnbindEvent = false;

    private void Awake()
    {
        InitUIButton();
    }

    private void OnDisable()
    {
        if (_isManualUnbindEvent == false)
        {
            Button_Base.onClick.RemoveAllListeners();
        }
    }

    private void InitUIButton()
    {
        if (Button_Base != null)
        {
            return;
        }

        var button = this.gameObject.GetComponentInChildren<Button>();
        if (button != null)
        {
            this.Button_Base = button;
        }
    }

    public void BindOnClickButtonEvent(UnityAction onClickCallback, bool isManualUnbindEvent = false)
    {
        if (Button_Base == null)
        {
            return;
        }

        Button_Base.onClick.AddListener(onClickCallback);
        _isManualUnbindEvent = isManualUnbindEvent;
    }

    public void UnBindOnClickButtonEvent(UnityAction onClickCallback)
    {
        if (Button_Base == null)
        {
            return;
        }

        Button_Base.onClick.RemoveListener(onClickCallback);
    }

    public void SetInteractable(bool isInteractable)
    {
        Button_Base.interactable = isInteractable;
    }

    public void SetSelected(bool isSelected)
    {
        if (Image_Selected == null)
        {
            return;
        }

        Image_Selected.gameObject.SetActive(isSelected);
    }
}