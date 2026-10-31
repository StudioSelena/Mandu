//스테이지 선택창. 스테이지를 골라 플레이에 들어가거나 로비로 돌아가는 UI
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageSelectUI : UIBase
{
    private const string StageNumberTextFormat = "STAGE {0}";
    private const string EmptyRecordText = "-"; //최고 등급, 최고 오차는 세이브가 생기기 전까지 비워 둔다

    [SerializeField] private UIButton Button_MainMenu;
    [SerializeField] private UIButton Button_Start;
    [SerializeField] private StageSlotUI Prefab_StageSlot;
    [SerializeField] private Transform Transform_StageSlotRoot;
    [SerializeField] private TMP_Text Text_StageNumber;
    [SerializeField] private TMP_Text Text_BestGradeValue;
    [SerializeField] private TMP_Text Text_BestErrorValue;
    [SerializeField] private TMP_Text Text_TryCountValue;

    private Dictionary<string, StageSlotUI> _stageSlotUIs = new Dictionary<string, StageSlotUI>(); //키는 스테이지Id
    private string _selectedStageId;

    //스테이지 데이터는 실행 중에 바뀌지 않으니 줄은 처음 한 번만 만든다
    private void Awake()
    {
        CreateAllStageSlots();
    }

    //플레이하고 돌아오면 바뀌는 값(고른 줄, 시도 횟수)은 열 때마다 다시 그린다
    private void OnEnable()
    {
        Button_MainMenu.BindOnClickButtonEvent(OnClick_MainMenu);
        Button_Start.BindOnClickButtonEvent(OnClick_Start);

        SelectStage(GameManager.Instance.GetCurrentStageId());
    }

    private void CreateAllStageSlots()
    {
        IReadOnlyList<StageData> stageDataList = GameDataManager.Instance.StageDataList;

        for (int i = 0; i < stageDataList.Count; i++)
        {
            string stageId = stageDataList[i].Id;
            string stageNumberText = string.Format(StageNumberTextFormat, i + GameManager.StageNumberOffset);

            StageSlotUI stageSlotUI = Instantiate(Prefab_StageSlot, Transform_StageSlotRoot);
            stageSlotUI.InitStageSlot(stageId, stageNumberText, SelectStage);
            _stageSlotUIs.Add(stageId, stageSlotUI);
        }
    }

    private void SelectStage(string stageId)
    {
        if (_stageSlotUIs.TryGetValue(stageId, out StageSlotUI selectedStageSlotUI) == false)
        {
            return;
        }

        _selectedStageId = stageId;

        foreach (KeyValuePair<string, StageSlotUI> stageSlotPair in _stageSlotUIs)
        {
            bool isSelected = stageSlotPair.Key == stageId;
            stageSlotPair.Value.SetSelected(isSelected);
        }

        Text_StageNumber.text = selectedStageSlotUI.StageNumberText;
        Text_BestGradeValue.text = EmptyRecordText;
        Text_BestErrorValue.text = EmptyRecordText;
        Text_TryCountValue.text = GameManager.Instance.GetStageTryCount(stageId).ToString();
    }

    private void OnClick_MainMenu()
    {
        GameManager.Instance.RequestOpenLobby();
    }

    private void OnClick_Start()
    {
        GameManager.Instance.RequestEnterGamePlay(_selectedStageId);
    }
}
