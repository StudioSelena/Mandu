//스테이지 선택창. 스테이지를 골라 플레이에 들어가거나 로비로 돌아가는 UI
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageSelectUI : UIBase
{
    private const string StageNumberTextFormat = "STAGE {0}";
    private const string EmptyRecordText = "-"; //아직 기록이 없는 스테이지의 최고 등급

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

    //플레이하고 돌아오면 바뀌는 값(고른 줄, 기록)은 열 때마다 다시 그린다
    private void OnEnable()
    {
        Button_MainMenu.BindOnClickButtonEvent(OnClick_MainMenu);
        Button_Start.BindOnClickButtonEvent(OnClick_Start);

        UpdateAllStageSlots();
        SelectStage(PlayRecordManager.Instance.GetLastPlayedStageId());
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

    //해금과 별은 플레이하고 돌아오면 바뀌므로 열 때마다 모든 줄에 다시 알린다
    private void UpdateAllStageSlots()
    {
        foreach (KeyValuePair<string, StageSlotUI> stageSlotPair in _stageSlotUIs)
        {
            bool isUnlocked = PlayRecordManager.Instance.IsStageUnlocked(stageSlotPair.Key);
            stageSlotPair.Value.SetStageUnlocked(isUnlocked);

            JudgmentGrade bestGrade = PlayRecordManager.Instance.GetStageBestGrade(stageSlotPair.Key);
            stageSlotPair.Value.SetStarCount(GameUtil.GetStarCount(bestGrade));
        }
    }

    //결과 화면 등급 이미지와 같은 영어 대문자로 보여준다
    private string GetBestGradeDisplayText(string stageId)
    {
        JudgmentGrade bestGrade = PlayRecordManager.Instance.GetStageBestGrade(stageId);
        if (bestGrade == JudgmentGrade.None)
        {
            return EmptyRecordText;
        }

        return bestGrade.ToString().ToUpperInvariant();
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
        Text_BestGradeValue.text = GetBestGradeDisplayText(stageId);
        Text_BestErrorValue.text = TimeUtil.GetTimingErrorDisplayText(PlayRecordManager.Instance.GetStageBestTimingErrorMs(stageId));
        Text_TryCountValue.text = PlayRecordManager.Instance.GetStageTryCount(stageId).ToString();
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
