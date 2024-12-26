using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace InteractiveViewer
{
    public class PKT_ModuleStructureUI : MonoBehaviour
    {
        [Header("General")]
        [SerializeField]
        private PKT_ButtonState buttonState;
        [SerializeField]
        private Light directionalLight;

        [Header("Hierarchy")]
        [SerializeField]
        private HierarchyItem hierarchyStructureList;
        [SerializeField]
        private HierarchyItem hierarchyStructureDetail;
        [SerializeField]
        private HierarchyItem hierarchyPartList;
        [SerializeField]
        private HierarchyItem hierarchyPartDetail;

        [Header("Structure")]
        [SerializeField]
        private TextMeshProUGUI structureTitle;
        [SerializeField]
        private Transform structureItemRoot;
        [SerializeField]
        private UIReference structureItemTemplate;
        [SerializeField]
        private UIReference structureDetailReference;
        [SerializeField]
        private MediaPreviewListUI structurePreviewListUI;

        [Header("Parts")]
        [SerializeField]
        private Transform partItemRoot;
        [SerializeField]
        private UIReference partItemTemplate;
        [SerializeField]
        private UIReference partDetailReference;
        [SerializeField]
        private MediaPreviewListUI partPreviewListUI;

        private ObjectPool<UIReference> _partItemPool;
        private PKT_ModulePathUI _pathUI;
        private int _partIndex = 0;

        public PKT_ModuleStructure SelectedStructure { get; private set; }
        public PKT_ModulePart SelectedPart { get; private set; }

        private IEnumerator Start()
        {
            _pathUI = FindFirstObjectByType<PKT_ModulePathUI>();

            //buttonState.onStateChanged.AddListener((value) => directionalLight.shadows = value ? LightShadows.None : LightShadows.Soft);
            hierarchyStructureList.onActivate.AddListener(() => _pathUI.AddPath(hierarchyStructureList));
            hierarchyStructureDetail.onActivate.AddListener(OnActivateStructureDetail);
            hierarchyPartList.onActivate.AddListener(() => _pathUI.AddPath(hierarchyPartList));
            hierarchyPartDetail.onActivate.AddListener(() => _pathUI.AddPath(hierarchyPartDetail));

            yield return new WaitUntil(GameManager.IsModuleValid);
            Initialize();
        }

        public void Initialize()
        {
            var str = GameManager.ActiveModule.structures;

            structureDetailReference.GetButton("btn.parts").onClick.AddListener(LoadStructureParts);
            for (int i = 0; i < str.Length; i++)
            {
                StartCoroutine(InitializeStructureItem(InstantiateAsync(structureItemTemplate, structureItemRoot), str[i]));
            }

            _partItemPool = new ObjectPool<UIReference>(partItemTemplate, 10, partItemRoot);
            partDetailReference.GetButton("btn.next").onClick.AddListener(() =>
            {
                _partIndex++;
                if (_partIndex >= SelectedStructure.parts.Length)
                    _partIndex = 0;
                LoadPartDetail();
            });
            partDetailReference.GetButton("btn.prev").onClick.AddListener(() =>
            {
                _partIndex--;
                if (_partIndex < 0)
                    _partIndex = SelectedStructure.parts.Length - 1;
                LoadPartDetail();
            });
        }

        public void ResetModelComponent()
        {
            SelectedStructure = null;
            SelectedPart = null;
            _partIndex = 0;
            if (GameManager.IsModelValid())
                GameManager.ActiveModel.SelectComponent();
        }

        private IEnumerator InitializeStructureItem(AsyncInstantiateOperation<UIReference> async, PKT_ModuleStructure structure)
        {
            while(!async.isDone) yield return new WaitForEndOfFrame();
            var item = async.Result[0];
            item.GetText("title").text = structure.Title;
            item.GetButton("button").onClick.AddListener(() => SelectStructure(structure));
            item.gameObject.SetActive(true);
        }

        private void OnActivateStructureDetail()
        {
            IEnumerator coroutineInternal()
            {
                _pathUI.AddPath(hierarchyStructureDetail);
                yield return new WaitForEndOfFrame(); // wait for updated selected structure
                if (SelectedStructure != null)
                    GameManager.ActiveModel.SelectComponent(SelectedStructure.componentKey);
            }
            StartCoroutine(coroutineInternal());
        }

        public void SelectStructure(PKT_ModuleStructure structure)
        {
            if (!hierarchyStructureDetail.isActive)
            {
                hierarchyStructureList.Deactivate(false);
                hierarchyStructureDetail.Activate(false);
            }

            if (SelectedStructure == structure) return;
            SelectedStructure = structure;

            _partIndex = 0;

            hierarchyStructureDetail.title = SelectedStructure.Title;
            _pathUI.RefreshLabel();

            structureDetailReference.GetText("title").SetText(SelectedStructure.Title);
            structureDetailReference.GetText("detail").SetText(SelectedStructure.Detail);
            structureDetailReference.GetButton("btn.parts").interactable = SelectedStructure.parts.Length > 0;
            LayoutRebuilder.ForceRebuildLayoutImmediate(structureDetailReference.GetText("detail").rectTransform);

            structurePreviewListUI.LoadPreviews(SelectedStructure.medias);
        }

        private void LoadStructureParts()
        {
            _partItemPool.ReturnAll();
            if (!hierarchyPartList.isActive)
            {
                hierarchyStructureDetail.Deactivate(false);
                hierarchyPartDetail.Activate(false);
            }

            structureTitle.text = SelectedStructure.Title;
            for (int i = 0; i < SelectedStructure.parts.Length; i++)
            {
                int index = i;
                PKT_ModulePart part = SelectedStructure.parts[index];

                UIReference item = _partItemPool.GetObject();
                item.transform.SetSiblingIndex(index);
                item.GetText("title").text = part.Title;
                item.GetButton("button").onClick.RemoveAllListeners();
                item.GetButton("button").onClick.AddListener(() =>
                {
                    _partIndex = index;
                    LoadPartDetail();
                });
            }
        }

        private void LoadPartDetail()
        {
            if (!hierarchyPartDetail.isActive)
            {
                hierarchyPartList.Deactivate(false);
                hierarchyPartDetail.Activate(false);
            }

            var part = SelectedStructure.parts[_partIndex];
            GameManager.ActiveModel.SelectComponent(part.componentKey);
            if (SelectedPart == part) return;
            SelectedPart = part;

            hierarchyPartDetail.title = SelectedPart.Title;
            _pathUI.RefreshLabel();


            partDetailReference.GetText("count").text = $"{_partIndex + 1} / {SelectedStructure.parts.Length}";
            partDetailReference.GetText("title").text = SelectedPart.Title;
            partDetailReference.GetText("detail").text = SelectedPart.Detail;
            LayoutRebuilder.ForceRebuildLayoutImmediate(partDetailReference.GetText("detail").rectTransform);

            partPreviewListUI.LoadPreviews(SelectedPart.medias);
        }
    }
}