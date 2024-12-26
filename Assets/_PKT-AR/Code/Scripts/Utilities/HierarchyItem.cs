using InteractiveViewer;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

[RequireComponent(typeof(LocalizeStringEvent))]
public class HierarchyItem : MonoBehaviour
{
    [Header("General")]
    public TextMeshProUGUI titleText;
    public bool autoSizeTitle;
    public Button backButton;
    public HierarchyItem[] children;

    [Header("Events")]
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    public bool isActive => gameObject.activeInHierarchy;
    public HierarchyItem parent { get; set; }
    public string title { get => titleText.text; 
        set
        {
            titleText.SetText(value);
            if(_localizedText != null && _localizedText.styleInfo != null)
                titleText.fontSize = value.Length > 100 ? _localizedText.styleInfo.preferedSize - 10 : _localizedText.styleInfo.preferedSize;
        } }

    private LocalizeStringEvent _localizedString;
    private PKT_ModulePathUI _pathUI;
    private PKT_LocalizedText _localizedText;

    public void Initialize()
    {
        _pathUI = FindFirstObjectByType<PKT_ModulePathUI>();
        if(titleText != null)
        {
            titleText.enableAutoSizing = autoSizeTitle;
            _localizedText = titleText.GetComponent<PKT_LocalizedText>();
        }
        _localizedString = GetComponent<LocalizeStringEvent>();
        _localizedString.OnUpdateString.AddListener(OnStringUpdate);

        if(parent && backButton)
        {
            backButton.onClick.AddListener(() =>
            {
                BackToParent();
            });
        }

        if (children == null)
            return;

        for (int i = 0; i < children.Length; i++)
        {
            children[i].parent = this;
            children[i].Initialize();
        }
    }

    private void OnStringUpdate(string value)
    {
        title = value;
    }

    public void SetStringReference(LocalizedString localizedString)
    {
        _localizedString.StringReference = localizedString;
    }

    public void Activate(bool activateParents)
    {
        gameObject.SetActive(true);
        if (activateParents)
            foreach (var e in GetAllParents())
                e.Activate(false);
        onActivate?.Invoke();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public void Deactivate(bool deactivateChildren)
    {
        gameObject.SetActive(false);
        if (deactivateChildren)
            foreach (var e in GetAllChildren())
                e.Deactivate(false);
        onDeactivate?.Invoke();
    }

    public void BackToParent()
    {
        Deactivate(false);
        parent.Activate(false);
    }

    public void ActivateChildIndex(int id)
    {
        if (id >= children.Length)
            return;

        Deactivate(false);
        children[id].Activate(false);
    }

    /// <summary>
    /// This function will always return a list
    /// </summary>
    /// <returns></returns>
    public List<HierarchyItem> GetAllParents()
    {
        HierarchyItem tParent = parent;
        List<HierarchyItem> allParents = new List<HierarchyItem>();
        while (tParent != null)
        {
            allParents.Add(parent);
            tParent = tParent.parent;
        }
        return allParents;
    }

    /// <summary>
    /// This function will always return a list
    /// </summary>
    /// <returns></returns>
    public List<HierarchyItem> GetAllChildren()
    {
        List<HierarchyItem> allChildren = new List<HierarchyItem>();
        GetChildrenRecursive(ref allChildren);
        return allChildren;
    }

    private void GetChildrenRecursive(ref List<HierarchyItem> allChildren)
    {
        if (children == null)
            return;

        allChildren.AddRange(children);
        for (int i = 0; i < children.Length; i++)
        {
            children[i].GetChildrenRecursive(ref allChildren);
        }
    }

    public string GetFullPath()
    {
        if (parent == null)
            return name;

        var parents = GetAllParents();
        string path = "";
        for (int i = parents.Count - 1; i >= 0; i--)
        {
            path += $"{parents[i].name}/";
        }
        path += name;
        return path;
    }
}
