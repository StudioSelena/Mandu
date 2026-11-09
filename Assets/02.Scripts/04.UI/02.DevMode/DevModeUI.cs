//개발자 패널. 스테이지를 골라 바로 시작한다. 에디터와 개발 빌드에서만 동작한다
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DevModeUI : UIBase
{
    //클래스 껍데기는 출시 빌드에도 남겨 프리팹의 스크립트 연결이 깨지지 않게 하고 안쪽만 뺀다
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] private TMP_Dropdown Dropdown_Stage;
    [SerializeField] private UIButton Button_StartStage;
    [SerializeField] private UIButton Button_UnlockAllStages;
    [SerializeField] private UIButton Button_ResetCurrentSlot;

    private void Awake()
    {
        FillStageDropdown();
    }

    private void OnEnable()
    {
        Button_StartStage.BindOnClickButtonEvent(OnClick_StartStage);
        Button_UnlockAllStages.BindOnClickButtonEvent(OnClick_UnlockAllStages);
        Button_ResetCurrentSlot.BindOnClickButtonEvent(OnClick_ResetCurrentSlot);
    }

    //엑셀 순서 그대로 채운다. 드롭다운 순번이 곧 GameManager의 스테이지 인덱스다
    private void FillStageDropdown()
    {
        IReadOnlyList<StageData> stageDataList = GameDataManager.Instance.StageDataList;
        List<string> optionTexts = new List<string>(stageDataList.Count);

        for (int i = 0; i < stageDataList.Count; i++)
        {
            int stageNumber = i + GameManager.StageNumberOffset;
            optionTexts.Add($"{stageNumber}. {stageDataList[i].Id}");
        }

        Dropdown_Stage.ClearOptions();
        Dropdown_Stage.AddOptions(optionTexts);
    }

    private void OnClick_StartStage()
    {
        GameManager.Instance.RequestStartStage(Dropdown_Stage.value);
    }

    //스테이지 선택창이 열려 있을 때만 바로 다시 그린다. 닫혀 있으면 다음에 열 때 OnEnable이 그린다
    private void RefreshOpenedStageSelectUI()
    {
        StageSelectUI stageSelectUI = UIManager.Instance.GetOpenedUI(UIType.StageSelectUI) as StageSelectUI;
        if (stageSelectUI == null)
        {
            return;
        }

        stageSelectUI.RefreshStageSelectUI();
    }

    private void OnClick_UnlockAllStages()
    {
        PlayRecordManager.Instance.UnlockAllStagesForDev();
        RefreshOpenedStageSelectUI();
    }

    private void OnClick_ResetCurrentSlot()
    {
        PlayRecordManager.Instance.ResetCurrentSlotForDev();
        RefreshOpenedStageSelectUI();
    }
#endif
}