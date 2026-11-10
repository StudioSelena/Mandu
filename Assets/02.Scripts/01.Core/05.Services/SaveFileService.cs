//세이브 슬롯 파일을 읽고 쓰는 서비스. PlayRecordManager가 만들어 들고 쓰며 다른 매니저는 모른다
using System.IO;
using UnityEngine;

public class SaveFileService
{
    private const string SaveFileNameFormat = "SaveSlot{0}.json"; //{0}은 슬롯 번호

    //Steam Auto-Cloud가 PC와 스팀덱에서 같은 파일로 알아보도록 persistentDataPath만 쓴다
    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, string.Format(SaveFileNameFormat, slotIndex));
    }

    public bool HasSaveFile(int slotIndex)
    {
        return File.Exists(GetSaveFilePath(slotIndex));
    }

    public void WriteSaveSlot(int slotIndex, SaveSlotModel saveSlotModel)
    {
        string saveFilePath = GetSaveFilePath(slotIndex);
        string json = JsonUtility.ToJson(saveSlotModel, true);
        File.WriteAllText(saveFilePath, json);

#if UNITY_EDITOR
        Debug.Log($"[SaveFileService] 저장 {saveFilePath}");
#endif
    }

    //파일이 없으면 null. 빈 슬롯에 새 모델을 만들지는 부른 쪽이 정한다
    public SaveSlotModel ReadSaveSlot(int slotIndex)
    {
        string saveFilePath = GetSaveFilePath(slotIndex);
        if (File.Exists(saveFilePath) == false)
        {
            return null;
        }

        string json = File.ReadAllText(saveFilePath);
        return JsonUtility.FromJson<SaveSlotModel>(json);
    }

    //개발자 모드 초기화와 세이브 삭제에서 쓴다. 파일이 없어도 File.Delete는 예외를 내지 않는다
    public void DeleteSaveSlot(int slotIndex)
    {
        string saveFilePath = GetSaveFilePath(slotIndex);
        File.Delete(saveFilePath);

#if UNITY_EDITOR
        Debug.Log($"[SaveFileService] 삭제 {saveFilePath}");
#endif
    }
}