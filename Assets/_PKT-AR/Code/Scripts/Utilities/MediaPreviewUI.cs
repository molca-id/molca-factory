using InteractiveViewer;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Molca;
using System.Collections;

public class MediaPreviewUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private GameObject failPreview;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private Button openImageButton;

    [DllImport("__Internal")]
    private static extern void openURL(string url);

    private Texture2D _cachedTexture;
    public PKT_MediaInfo MediaInfo { get; private set; }

    private void Start()
    {
        openImageButton.onClick.AddListener(OpenMedia);
    }

    private void OnDestroy()
    {
        Clear();
    }

    public void LoadPreview(PKT_MediaInfo info, bool cacheTexture = false)
    {
        if (info == null)
            failPreview.SetActive(true);

        if (MediaInfo == info)
            return;
        Clear();

        MediaInfo = info;
        loadingUI.SetActive(true);

        void onGetTextureInternal(Texture2D texture)
        {
            if (info != MediaInfo) // Check if preview has ben refreshed with a new media info
            {
                Debug.Log($"Abort preview operation, media missmatch, op: {info.id}, current: {MediaInfo?.id}");
                return;
            }

            loadingUI.SetActive(false);
            if (cacheTexture)
                _cachedTexture = CopyTexture(texture);

            if (texture)
                SetTexture(cacheTexture ? _cachedTexture : texture);
            else
                failPreview.SetActive(true);
        }
        RuntimeManager.RunCoroutine(MediaInfo.GetTexture(onGetTextureInternal));
    }



    public void SetTexture(Texture2D texture)
    {
        rawImage.texture = texture;
        rawImage.gameObject.SetActive(true);
        rawImage.GetComponent<AspectRatioFitter>().aspectRatio = texture.width / (float)texture.height;
        openImageButton.gameObject.SetActive(true);
    }

    public void Clear()
    {
        if (MediaInfo != null)
        {
            MediaInfo.Unload();
            MediaInfo = null;
        }

        if(_cachedTexture != null)
            Destroy(_cachedTexture);

        rawImage.gameObject.SetActive(false);
        openImageButton.gameObject.SetActive(false);
        failPreview.SetActive(false);
    }
    private void OpenMedia()
    {
        if (MediaInfo != null)
        {
            switch (MediaInfo.type)
            {
                case PKT_MediaInfo.Type.Image:
                    RuntimeManager.RunCoroutine(ImageHandler.Load(MediaInfo));
                    break;
                case PKT_MediaInfo.Type.Video:
                    VideoHandler.Load(MediaInfo);
                    break;
                case PKT_MediaInfo.Type.Document:
                    openURL(MediaInfo.url);
                    break;
                default:
                    break;
            }
        }
        else if (rawImage.texture != null)
        {
            ImageHandler.Load(rawImage.texture);
        }
    }

    private Texture2D CopyTexture(Texture2D sourceTexture)
    {
        // Create a new texture with the same dimensions and format
        Texture2D newTexture = new Texture2D(sourceTexture.width, sourceTexture.height,
            sourceTexture.format, false);

        // Direct copy of texture
        Graphics.CopyTexture(sourceTexture, 0, 0, newTexture, 0, 0);

        return newTexture;
    }
}
