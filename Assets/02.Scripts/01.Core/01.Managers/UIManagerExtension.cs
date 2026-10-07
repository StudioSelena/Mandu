// UIManager 확장 메서드 모음. UI 추가 시 이 파일에만 추가한다
public static class UIManagerExtension
{
    public static string GetUIPath(this UIManager uiManager, UIRootType rootType, UIType uiType)
    {
        string path = $"UI/{rootType}/{uiType}";
        return path;
    }
}