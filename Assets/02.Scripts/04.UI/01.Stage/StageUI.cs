//플레이 중 화면. 삶기 버튼, 시간 표시, 입력 안내를 맡는 UI
using UnityEngine;

public class StageUI : UIBase
{
    [SerializeField] private UIButton Button_Boil;

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
                break;

            case StageState.Boiling:
                Button_Boil.gameObject.SetActive(false);
                break;
        }
    }
}