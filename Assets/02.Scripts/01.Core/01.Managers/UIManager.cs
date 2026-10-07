// UI 생성과 오픈, 클로즈를 관리하는 매니저
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonBase<UIManager>
{
    [SerializeField] private Canvas Canvas_BgRoot;
    [SerializeField] private Canvas Canvas_MainRoot;
    [SerializeField] private Canvas Canvas_ContentRoot;
    [SerializeField] private Canvas Canvas_PopupRoot;
    [SerializeField] private Canvas Canvas_VeryFrontRoot;

    private Dictionary<UIType, UIBase> _createdUI = new Dictionary<UIType, UIBase>();
    private HashSet<UIType> _openedUI = new HashSet<UIType>();

    public UIBase OpenUI(UIRootType rootType, UIType uiType, bool isActive = true)
    {
        UIBase openedUI = GetCreatedUI(rootType, uiType);

        if (openedUI == null)
        {
            Debug.LogError($"UI를 열 수 없음. UIType : {uiType}");
            return null;
        }

        if (_openedUI.Contains(uiType) == false)
        {
            openedUI.gameObject.SetActive(isActive);
            _openedUI.Add(uiType);
        }
        else if (isActive == true && openedUI.gameObject.activeSelf == false)
        {
            openedUI.gameObject.SetActive(true);
        }

        return openedUI;
    }

    public void CloseUI(UIType uiType)
    {
        if (_openedUI.Contains(uiType) == false)
        {
            return;
        }

        if (_createdUI.TryGetValue(uiType, out UIBase openedUI) == false)
        {
            _openedUI.Remove(uiType);
            return;
        }

        if (openedUI != null)
        {
            openedUI.gameObject.SetActive(false);
        }

        _openedUI.Remove(uiType);
    }

    public UIBase GetOpenedUI(UIType uiType)
    {
        if (_openedUI.Contains(uiType) == false)
        {
            return null;
        }

        if (_createdUI.TryGetValue(uiType, out UIBase openedUI) == false)
        {
            return null;
        }

        return openedUI;
    }

    public bool IsOpenedUI(UIType uiType)
    {
        return _openedUI.Contains(uiType);
    }

    private Transform GetRootTransform(UIRootType rootType)
    {
        Transform root = null;

        switch (rootType)
        {
            case UIRootType.Bg:
                root = Canvas_BgRoot.transform;
                break;

            case UIRootType.Main:
                root = Canvas_MainRoot.transform;
                break;

            case UIRootType.Content:
                root = Canvas_ContentRoot.transform;
                break;

            case UIRootType.Popup:
                root = Canvas_PopupRoot.transform;
                break;

            case UIRootType.VeryFront:
                root = Canvas_VeryFrontRoot.transform;
                break;
        }

        return root;
    }

    private void CreateUI(UIRootType rootType, UIType uiType)
    {
        if (_createdUI.ContainsKey(uiType) == true)
        {
            return;
        }

        string path = this.GetUIPath(rootType, uiType);
        GameObject loadedObj = Resources.Load<GameObject>(path);

        if (loadedObj == null)
        {
            Debug.LogError($"UI 프리팹을 찾을 수 없음. Path : {path}");
            return;
        }

        Transform rootTransform = GetRootTransform(rootType);

        if (rootTransform == null)
        {
            Debug.LogError($"UI Root를 찾을 수 없음. Root : {rootType}");
            return;
        }

        GameObject createdObject = Instantiate(loadedObj, rootTransform);
        UIBase uiBase = createdObject.GetComponent<UIBase>();

        if (uiBase == null)
        {
            Debug.LogError($"UIBase 컴포넌트가 없음. UIType : {uiType}");
            Destroy(createdObject);
            return;
        }

        _createdUI.Add(uiType, uiBase);
    }

    private UIBase GetCreatedUI(UIRootType rootType, UIType uiType)
    {
        if (_createdUI.ContainsKey(uiType) == false)
        {
            CreateUI(rootType, uiType);
        }

        if (_createdUI.TryGetValue(uiType, out UIBase createdUI) == false)
        {
            return null;
        }

        return createdUI;
    }
}