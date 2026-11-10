//플레이 중 화면. 삶기 버튼, 시간 표시, 입력 안내를 맡는 UI
using TMPro;
using UnityEngine;

public class StageUI : UIBase
{
    [SerializeField] private UIButton Button_Boil;
    [SerializeField] private GameObject Row_TargetTime;
    [SerializeField] private TMP_Text Text_TargetTime;
    [SerializeField] private GameObject Row_InputTime;
    [SerializeField] private TMP_Text Text_InputTime;
    [SerializeField] private TMP_Text Text_StageNumber;

    private void OnEnable()
    {
        Button_Boil.BindOnClickButtonEvent(OnClick_Boil, true);
        StageManager.Instance.StageStateChangedEvent += HandleStageStateChanged;
    }

    private void OnDisable()
    {
        Button_Boil.UnBindOnClickButtonEvent(OnClick_Boil);

        if (StageManager.Instance == null)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent -= HandleStageStateChanged;
    }

    private void OnClick_Boil()
    {
        StageManager.Instance.RequestStartBoiling();
    }

    private void HandleStageStateChanged(StageState stageState)
    {
        switch (stageState)
        {
            case StageState.Ready:
                Button_Boil.gameObject.SetActive(true);
                ShowStageNumber();
                ShowTargetTime();
                Row_InputTime.SetActive(false);
                break;

            case StageState.Boiling:
                Button_Boil.gameObject.SetActive(false);
                Row_InputTime.SetActive(false);
                break;

            case StageState.JudgmentComplete:
                ShowTargetTime();
                ShowInputTime();
                break;

            case StageState.ShowingResult:
                Row_TargetTime.SetActive(false);
                Row_InputTime.SetActive(false);
                break;
        }
    }

    private void ShowTargetTime()
    {
        Text_TargetTime.text = TimeUtil.GetSecondsDisplayText(StageManager.Instance.CurrentTargetTimeMs);
        Row_TargetTime.SetActive(true);
    }

    private void ShowInputTime()
    {
        Text_InputTime.text = TimeUtil.GetInputTimeDisplayText(StageManager.Instance.CurrentInputTimeMs);
        Row_InputTime.SetActive(true);
    }

    private void ShowStageNumber()
    {
        Text_StageNumber.text = StageManager.Instance.CurrentStageNumber.ToString();
    }
}