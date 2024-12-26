using UnityEngine;
using Molca;
using UnityEngine.UI;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;

namespace InteractiveViewer
{
    public class PKT_ModuleDocumentUI : MonoBehaviour
    {
        [Header("Hierarchy")]
        [SerializeField] private Button jsaButton;
        [SerializeField] private HierarchyItem hierarchyJSAList;
        [SerializeField] private HierarchyItem hierarchyDocList;
        [SerializeField] private HierarchyItem hierarchySubDocList;

        [Header("Document")]
        [SerializeField] private Transform jsaItemRoot;
        [SerializeField] private Transform docItemRoot;
        [SerializeField] private UIReference docItemTemplate;

        [Header("Sub-Document")]
        [SerializeField] private Transform subDocItemRoot;
        [SerializeField] private UIReference subDocItemTemplate;

        [DllImport("__Internal")]
        private static extern void openURL(string url);

        private PKT_MediaInfo _selectedDocument;
        private PKT_ModulePathUI _path;
        private ObjectPool<UIReference> _subDocPool;

        private void Start()
        {
            _subDocPool = new ObjectPool<UIReference>(subDocItemTemplate, 2, subDocItemRoot);
            _path = FindFirstObjectByType<PKT_ModulePathUI>();
            hierarchySubDocList.onActivate.AddListener(() => _path.AddPath(hierarchySubDocList));

            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            yield return new WaitUntil(RuntimeManager.IsReady);
            yield return new WaitUntil(GameManager.IsModuleValid);

            var jsas = GameManager.ActiveModule.jsas;
            var docs = GameManager.ActiveModule.documentations;

            if (jsas == null || jsas.Count == 0)
            {
                jsaButton.interactable = false;
            }
            else
            {
                for (var i = 0; i < jsas.Count; i++)
                {
                    var async = InstantiateAsync(subDocItemTemplate, jsaItemRoot);
                    while (!async.isDone) yield return new WaitForEndOfFrame();
                    var item = async.Result[0];
                    item.GetText("title").SetText(jsas[i].name);
                    item.GetButton("button").onClick.AddListener(() => _selectedDocument = jsas[i]);
                    item.gameObject.SetActive(true);
                }
            }

            for (var i = 0; i < docs.Count; i++)
            {
                var cat = docs[i];
                var async = InstantiateAsync(docItemTemplate, docItemRoot);
                while (!async.isDone) yield return new WaitForEndOfFrame();
                var item = async.Result[0];
                item.GetText("title").SetText(cat.title);
                item.GetButton("button").onClick.AddListener(() => LoadSubDocument(cat));
                item.gameObject.SetActive(true);
            }
        }

        private void LoadSubDocument(PKT_ModuleDocumentation doc)
        {
            hierarchySubDocList.title = doc.title;
            _path.RefreshLabel();

            _subDocPool.ReturnAll();
            for (var i = 0; i < doc.subDocs.Count; i++)
            {
                var subDoc = doc.subDocs[i];
                var item = _subDocPool.GetObject();
                item.GetText("title").SetText(subDoc.name);
                item.GetButton("button").onClick.RemoveAllListeners();
                item.GetButton("button").onClick.AddListener(() => _selectedDocument = subDoc);
            }
        }

        public void LoadSelected()
        {
            IEnumerator coroutineInternal()
            {
                yield return new WaitForEndOfFrame(); // Wait for selection event performed
                if (_selectedDocument == null)
                    yield break;

#if UNITY_WEBGL
                openURL(_selectedDocument.url);
#else
            await DocumentHandler.Load(
                Path.Combine(Application.streamingAssetsPath, _selectedDocument.url),
                _selectedDocument.name);
#endif
            }
            StartCoroutine(coroutineInternal());

            /*if (await DocumentHandler.Load(_selectedDocument) == false) // reset media url if asset loading failed
                _selectedDocument.url = null;*/
        }
    }
}