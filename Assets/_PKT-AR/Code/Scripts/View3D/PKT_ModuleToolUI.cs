using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace InteractiveViewer
{
    public class PKT_ModuleToolUI : MonoBehaviour
    {
        [Header("Hierarchy")]
        [SerializeField] private HierarchyItem hierarchyToolList;
        [SerializeField] private HierarchyItem hierarchyToolDetail;

        [Header("Tool List")]
        [SerializeField] private Transform toolItemRoot;
        [SerializeField] private UIReference toolItemTemplate;

        [Header("Tool Detail")]
        [SerializeField] private UIReference toolDetailUI;
        [SerializeField] private MediaPreviewListUI mediaPreviewListUI;
        [SerializeField] private Button nextToolButton;
        [SerializeField] private Button previousToolButton;

        private PKT_ModulePathUI _pathUI;
        private PKT_ToolInfo[] _toolInfos;
        private int _toolIndex;

        private void Start()
        {
            _pathUI = FindFirstObjectByType<PKT_ModulePathUI>();
            hierarchyToolList.onActivate.AddListener(() => _pathUI.AddPath(hierarchyToolList));
            hierarchyToolDetail.onActivate.AddListener(() => _pathUI.AddPath(hierarchyToolDetail));

            StartCoroutine(Initialize());
        }

        IEnumerator Initialize()
        {
            //Debug.Log("Initializing tools UI.");
            yield return new WaitUntil(GameManager.IsModuleValid);

            _toolInfos = GameManager.ActiveModule.tools;
            for (int i = 0; i < _toolInfos.Length; i++)
            {
                int index = i;
                var info = _toolInfos[index];
                //Debug.Log($"Initiating tool id: {info.id}");
                info.Init();
                var async = InstantiateAsync(toolItemTemplate, toolItemRoot);
                while(!async.isDone) yield return new WaitForEndOfFrame();
                var item = async.Result[0];
                item.gameObject.SetActive(true);
                item.GetButton("button").onClick.AddListener(() => LoadToolDetail(index));
                item.GetText("title").SetText(info.title);
                //Debug.Log($"Finished init tool item {info.id}");
            }

            nextToolButton.onClick.AddListener(NextTool);
            previousToolButton.onClick.AddListener(PreviousTool);
            //Debug.Log("Tools UI initialized.");
        }

        private void PreviousTool()
        {
            LoadToolDetail(--_toolIndex);
        }

        private void NextTool()
        {
            LoadToolDetail(++_toolIndex);
        }

        private void LoadToolDetail(int index)
        {
            Debug.Log($"Loading tool detail index: {index}");
            _toolIndex = index;
            if(_toolIndex < 0) _toolIndex = _toolInfos.Length - 1;
            else if(_toolIndex >=  _toolInfos.Length) _toolIndex = 0;

            var info = _toolInfos[_toolIndex];

            hierarchyToolDetail.title = info.title;
            _pathUI.RefreshLabel();

            toolDetailUI.GetText("title").SetText(info.title);
            toolDetailUI.GetText("detail").SetText(info.detail);
            LayoutRebuilder.ForceRebuildLayoutImmediate(toolDetailUI.GetText("detail").rectTransform);

            mediaPreviewListUI.LoadPreviews(info.medias);
        }
    }
}