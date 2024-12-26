using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace InteractiveViewer
{
    public class PKT_ModulePathUI : MonoBehaviour
    {
        [Serializable]
        private class PathReference
        {
            public UIReference UIReference;
            public SimpleToggle toggle;
            public PKT_ButtonState buttonState;

            public HierarchyItem HierarchyItem { get; private set; }

            public void AssignInfo(HierarchyItem item)
            {
                HierarchyItem = item;
                Refresh();
            }

            public void Refresh()
            {
                string label = HierarchyItem ? HierarchyItem.title : "...";
                if (label.Length > 60)
                    label = label.Remove(60) + "...";
                UIReference.GetText("label").SetText(label);
                LayoutRebuilder.ForceRebuildLayoutImmediate(UIReference.transform as RectTransform);
            }
        }

        [SerializeField]
        private HierarchyItem hierarchyRoot;
        [SerializeField]
        private PathReference[] pathReferences;

        [Header("Breadcrumb")]
        [SerializeField]
        private UIReference breadcrumbTemplate;
        [SerializeField]
        private GameObject breadcrumbRoot;

        private RectTransform _rectTransform;
        private List<UIReference> _breadcrumbs;
        private Stack<HierarchyItem> path = new Stack<HierarchyItem>();

        private void Start()
        {
            _rectTransform = transform as RectTransform;
            _breadcrumbs = new List<UIReference>();

            for (int i = 0; i < pathReferences.Length; i++)
            {
                var reference = pathReferences[i];
                reference.buttonState.onStateOn.AddListener(() =>
                {
                    if (reference.HierarchyItem != null)
                        BackTo(reference.HierarchyItem);
                });

                if (i == 0) // Handle breadcrumb event only if path count > 2
                {
                    reference.buttonState.onStateChanged.AddListener(state =>
                    {
                        if (path.Count > 2)
                        {
                            breadcrumbRoot.SetActive(state);
                            if (state)
                                hierarchyRoot.Deactivate(true);
                        }
                    });
                }
            }

            Refresh();
        }

        private void Refresh()
        {
            bool useBc = path.Count > 2;
            if (useBc)
            {
                pathReferences[0].AssignInfo(null);
                pathReferences[1].AssignInfo(path.ElementAt(0));
                pathReferences[1].buttonState.SetState(true);

                int loopLength = Math.Max(_breadcrumbs.Count, path.Count) - 1;
                for (int i = 0; i < loopLength; i++)
                {
                    if (i == _breadcrumbs.Count) // Instantiate breadcrumb item if it's less than needed
                        _breadcrumbs.Add(Instantiate(breadcrumbTemplate, breadcrumbRoot.transform));
                    else if (i >= path.Count) // Deactivate breadcrumb item if it's more than needed
                    {
                        _breadcrumbs[i].gameObject.SetActive(false);
                        _breadcrumbs[i].GetButton("button").onClick.RemoveAllListeners();
                        continue;
                    }

                    var item = path.ElementAt(path.Count - (i + 1)); // Reverse indexing for stack

                    _breadcrumbs[i].gameObject.SetActive(true);
                    _breadcrumbs[i].GetText("label").SetText(item.title);
                    _breadcrumbs[i].GetButton("button").onClick.RemoveAllListeners();
                    _breadcrumbs[i].GetButton("button").onClick.AddListener(() =>
                    {
                        breadcrumbRoot.gameObject.SetActive(false);
                        hierarchyRoot.Activate(false);
                        item.Activate(false);
                        BackTo(item);
                    });
                }
            }
            else
            {
                int loopLength = pathReferences.Length - 1;
                for (int i = 0; i <= loopLength; i++)
                {
                    if (i < path.Count)
                    {
                        var item = path.ElementAt(path.Count - (i + 1)); // Reverse indexing for stack
                        pathReferences[i].AssignInfo(item);
                        pathReferences[i].toggle.SetState(true);
                        if (i == path.Count - 1)
                        {
                            pathReferences[i].buttonState.SetState(true);
                        }
                    }
                    else
                    {
                        pathReferences[i].AssignInfo(null);
                        pathReferences[i].toggle.SetState(false);
                    }
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }

        public void RefreshLabel()
        {
            foreach (var item in pathReferences)
            {
                item.Refresh();
            }
        }

        public void AddPath(HierarchyItem item)
        {
            if (path.Contains(item))
            {
                if(path.Peek() != item)
                    BackTo(item);
                return;
            }

            path.Push(item);
            Refresh();
        }

        public void BackTo(HierarchyItem item)
        {
            if (path.Peek() == item)
            {
                if(!item.isActive) // Check if item is already at the top
                {
                    if(!hierarchyRoot.isActive) // Check if root is active
                        hierarchyRoot.Activate(false);
                    item.Activate(false);
                }
                return;
            }

            if (!path.Contains(item))
            {
                Debug.LogWarning($"Failed to go back, no item {item} in the path");
                return;
            }

            while (path.Peek() != item)
                path.Pop().BackToParent();

            Refresh();
        }

        public void Clear(bool refresh = true)
        {
            if (GameManager.IsModelValid())
                GameManager.ActiveModel.SelectComponent();

            while (path.Count > 0)
                path.Pop().BackToParent();
            if (refresh)
                Refresh();
        }
    }
}