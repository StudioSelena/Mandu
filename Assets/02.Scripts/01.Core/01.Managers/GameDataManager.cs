//JSON 데이터 표를 읽어 Dictionary로 보관하고 Id로 조회하게 해 주는 매니저
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : SingletonBase<GameDataManager>
{
    private const string JsonFolderPath = "JsonOutput/";

    private Dictionary<string, StageData> _stageDataDictionary = new Dictionary<string, StageData>();
    private Dictionary<string, TargetTimeData> _targetTimeDataDictionary = new Dictionary<string, TargetTimeData>();
    private List<StageData> _stageDataList = new List<StageData>(); //엑셀 행 순서가 곧 스테이지 순서

    //스테이지 순서 목록. 밖에서는 읽기만 가능
    public IReadOnlyList<StageData> StageDataList
    {
        get
        {
            return _stageDataList;
        }
    }

    //JsonUtility는 [ ]로 시작하는 JSON을 바로 읽지 못해서 { "items": [ ] } 모양으로 감싸서 읽는다
    [Serializable]
    private class SerializationWrapper<T>
    {
        public List<T> items;
    }

    protected override void Awake()
    {
        base.Awake();

        //중복 매니저라서 base.Awake에서 파괴되는 경우에는 로드하지 않는다
        if (Instance != this)
        {
            return;
        }

        LoadAllGameData();
    }

    private void LoadAllGameData()
    {
        List<TargetTimeData> targetTimeDataList = LoadDataList<TargetTimeData>(nameof(TargetTimeData));
        _targetTimeDataDictionary = BuildDataDictionary(targetTimeDataList);

        _stageDataList = LoadDataList<StageData>(nameof(StageData));
        _stageDataDictionary = BuildDataDictionary(_stageDataList);
        ValidateStageTargetTimeIds();

#if UNITY_EDITOR
        Debug.Log($"[GameDataManager] TargetTimeData {_targetTimeDataDictionary.Count}개, StageData {_stageDataDictionary.Count}개 로드");
#endif
    }

    //표 하나를 읽어 행 순서 그대로 List로 돌려준다. 실패하면 빈 List
    private List<T> LoadDataList<T>(string tableName) where T : GameDataBase
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(JsonFolderPath + tableName);

        if (jsonFile == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"[GameDataManager] JSON 파일이 없습니다. Resources/{JsonFolderPath}{tableName}");
#endif
            return new List<T>();
        }

        string wrappedJson = "{\"items\":" + jsonFile.text + "}";
        Resources.UnloadAsset(jsonFile);

        SerializationWrapper<T> wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

        if (wrapper == null || wrapper.items == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"[GameDataManager] JSON을 읽지 못했습니다. {tableName}");
#endif
            return new List<T>();
        }

        return wrapper.items;
    }

    //List를 Id 키 Dictionary로 바꾼다. Id가 비었거나 겹치는 행은 건너뛰고 경고한다
    private Dictionary<string, T> BuildDataDictionary<T>(List<T> dataList) where T : GameDataBase
    {
        Dictionary<string, T> dataDictionary = new Dictionary<string, T>();

        foreach (T data in dataList)
        {
            if (string.IsNullOrEmpty(data.Id))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[GameDataManager] {typeof(T).Name}에 Id가 빈 행이 있어 건너뜁니다.");
#endif
                continue;
            }

            if (dataDictionary.ContainsKey(data.Id))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[GameDataManager] {typeof(T).Name}에 중복 Id가 있어 뒤의 행을 건너뜁니다. {data.Id}");
#endif
                continue;
            }

            dataDictionary.Add(data.Id, data);
        }

        return dataDictionary;
    }

    //모든 스테이지의 TargetTimeId가 목표 시간 표에 있는지 검사한다. 없으면 에러로 알린다
    private void ValidateStageTargetTimeIds()
    {
        foreach (StageData stageData in _stageDataList)
        {
            if (string.IsNullOrEmpty(stageData.TargetTimeId))
            {
#if UNITY_EDITOR
                Debug.LogError($"[GameDataManager] {stageData.Id}의 TargetTimeId가 비어 있습니다.");
#endif
                continue;
            }

            if (!_targetTimeDataDictionary.ContainsKey(stageData.TargetTimeId))
            {
#if UNITY_EDITOR
                Debug.LogError($"[GameDataManager] {stageData.Id}가 목표 시간 표에 없는 Id를 가리킵니다. {stageData.TargetTimeId}");
#endif
            }
        }
    }

    //Id로 스테이지 데이터를 찾는다. Id가 비었거나 없으면 null
    public StageData GetStageData(string stageId)
    {
        if (string.IsNullOrEmpty(stageId))
        {
#if UNITY_EDITOR
            Debug.LogWarning("[GameDataManager] 빈 Id로 StageData를 찾으려 했습니다.");
#endif
            return null;
        }

        if (_stageDataDictionary.TryGetValue(stageId, out StageData stageData))
        {
            return stageData;
        }

#if UNITY_EDITOR
        Debug.LogWarning($"[GameDataManager] StageData에 없는 Id입니다. {stageId}");
#endif
        return null;
    }

    //Id로 목표 시간 데이터를 찾는다. Id가 비었거나 없으면 null
    public TargetTimeData GetTargetTimeData(string targetTimeId)
    {
        if (string.IsNullOrEmpty(targetTimeId))
        {
#if UNITY_EDITOR
            Debug.LogWarning("[GameDataManager] 빈 Id로 TargetTimeData를 찾으려 했습니다.");
#endif
            return null;
        }

        if (_targetTimeDataDictionary.TryGetValue(targetTimeId, out TargetTimeData targetTimeData))
        {
            return targetTimeData;
        }

#if UNITY_EDITOR
        Debug.LogWarning($"[GameDataManager] TargetTimeData에 없는 Id입니다. {targetTimeId}");
#endif
        return null;
    }
}