using UnityEngine;
using UnityEngine.UI;
using InteractiveViewer;
using UnityEngine.UI.Extensions;

public class MediaPreviewListUI : MonoBehaviour
{
    public GameObject noMediaUI;
    public MediaPreviewUI previewPrefab;
    public Toggle previewTogglePrefab;

    public Transform previewRoot;
    public Transform previewToggleRoot;

    private HorizontalScrollSnap _scrollSnap;
    private PaginationManager _paginationManager;

    private ObjectPool<MediaPreviewUI> _previewPool;
    private ObjectPool<Toggle> _previewTogglePool;

    private void Awake()
    {
        _scrollSnap = GetComponent<HorizontalScrollSnap>();
        _paginationManager = GetComponentInChildren<PaginationManager>(true);

        _scrollSnap.enabled = false;
        _paginationManager.enabled = false;

        _previewPool = new ObjectPool<MediaPreviewUI>(previewPrefab, 5, previewRoot);
        _previewTogglePool = new ObjectPool<Toggle>(previewTogglePrefab, 1, previewToggleRoot);

        _previewPool.onObjectReturned += OnObjectReturned;
    }

    public void LoadPreviews(PKT_MediaInfo[] infos)
    {
        Clear();

        bool isValid = infos != null && infos.Length > 0;
        noMediaUI.SetActive(!isValid);
        gameObject.SetActive(isValid);

        if (!isValid)
            return;

        for (int i = 0; i < infos.Length; i++)
        {
            var item = _previewPool.GetObject();
            item.LoadPreview(infos[i]);
            _scrollSnap.AddChild(item.gameObject);
            item.transform.localScale = Vector3.one;
            _previewTogglePool.GetObject();            
        }

        _scrollSnap.enabled = true;
        _paginationManager.enabled = true;
        _scrollSnap.UpdateLayout();
        _paginationManager.ResetPaginationChildren();
    }

    public void Clear()
    {
        _scrollSnap.enabled = false;
        _paginationManager.enabled = false;

        if (_scrollSnap.ChildObjects.Length > 0 && _paginationManager.AnyTogglesOn())
            _scrollSnap.GoToScreen(0);
        _previewPool.ReturnAll();
        _previewTogglePool.ReturnAll();
    }

    private void OnObjectReturned(MediaPreviewUI preview)
    {
        preview.transform.SetParent(null);
    }
}
