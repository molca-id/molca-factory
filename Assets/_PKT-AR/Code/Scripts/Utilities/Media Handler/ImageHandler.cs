using UnityEngine;
using UnityEngine.UI;
using Molca;
using InteractiveViewer;
using System;
using TMPro;
using System.Collections;

public class ImageHandler : MonoBehaviour
{
    private static ImageHandler _instance;

    [SerializeField]
    private TextMeshProUGUI title;
    [SerializeField]
    private RectTransform imageContainer;
    [SerializeField]
    private RawImage targetImage;
    [SerializeField]
    private AspectRatioFitter backgroundRatioFitter;

    [Header("Visibility")]
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Image rootBackground;

    private PKT_MediaInfo _mediaInfo;

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.Log($"{typeof(ImageHandler)} already exist: {_instance}");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        closeButton.onClick.AddListener(CloseViewer);
        CloseViewer();
    }

    private void OnDisable()
    {
        Clear();
    }

    public static IEnumerator Load(PKT_MediaInfo info)
    {
        _instance.title.text = string.IsNullOrWhiteSpace(info.name) ? "Image Viewer" : info.name;
        yield return info.GetTexture(_instance.SetTextureInternal);
        _instance._mediaInfo = info;
    }

    public static void Load(Texture texture)
    {
        _instance.SetTextureInternal(texture);
    }

    private void SetTextureInternal(Texture txt2d)
    {
        if (txt2d == null)
            return;

        targetImage.texture = txt2d;
        targetImage.GetComponent<AspectRatioFitter>().aspectRatio = txt2d.width / (float)txt2d.height;
        backgroundRatioFitter.aspectRatio = txt2d.width / (txt2d.height * 1.05f);

        ToggleVisibility(true);
    }

    private void CloseViewer()
    {
        ToggleVisibility(false);
    }

    private void ToggleVisibility(bool visible)
    {
        rootBackground.enabled = visible;
        rootPanel.SetActive(visible);
        Clear();
    }

    private void Clear()
    {
        if (_mediaInfo == null)
            return;
        _mediaInfo.Unload();
        _mediaInfo = null;
    }

    /// <summary>
    /// Auto close viewer if application loses focus
    /// </summary>
    /// <param name="focus"></param>
    private void OnApplicationFocus(bool focus)
    {
        if (focus) return;
        CloseViewer();
    }
}
