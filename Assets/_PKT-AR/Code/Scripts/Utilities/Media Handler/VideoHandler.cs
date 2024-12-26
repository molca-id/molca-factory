using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using InteractiveViewer;
using UnityEngine.UI;
using TMPro;
using System;
using Molca;

public class VideoHandler : MonoBehaviour
{
    public static VideoHandler _instance;

    [SerializeField]
    private TextMeshProUGUI title;
    [SerializeField] private VideoPlayer player;
    [SerializeField]
    private RectTransform imageContainer;
    [SerializeField]
    private RawImage targetImage;
    [SerializeField]
    private AspectRatioFitter backgroundRatioFitter;
    [SerializeField]
    private CanvasGroup canvasGroup;

    [Header("Visibility")]
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject rootPanel;
    //[SerializeField] private Image rootBackground;

    [Header("Playback Control")]
    [SerializeField] private PKT_ButtonState playButtonState;
    [SerializeField] private Button stopButton;
    [SerializeField] private ProgressBarUI playbackBar;

    private float _lastRefresh;
    private bool _isPlayingInternal;
    private static bool IsPlaying => _instance.player.isPlaying;

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.Log($"{typeof(VideoHandler)} already exist: {_instance}");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        playbackBar.UpdateProgress(0f);
        closeButton.onClick.AddListener(CloseViewer);
        playButtonState.onStateChanged.AddListener(TogglePlayState);
        playbackBar.onValueChanged.AddListener(SetVideoProgress);
        playbackBar.onBeginDrag += () =>
        {
            if (_isPlayingInternal) player.Pause();
        };
        playbackBar.onEndDrag += () =>
        {
            if (_isPlayingInternal) player.Play();
        };
        stopButton.onClick.AddListener(() =>
        {
            if (!player.isPrepared) return;
            playButtonState.isOn = false;
            player.Stop();
            playbackBar.UpdateProgress(0f);
        });

        player.prepareCompleted += OnPreparationCompleted;

        CloseViewer();
    }

    private void LateUpdate()
    {
        if (Time.time - _lastRefresh < .05f)
            return;

        RefreshBar();
        _lastRefresh = Time.time;
    }

    private void OnPreparationCompleted(VideoPlayer source)
    {
        player.time = 0;
        playButtonState.isOn = true; // this will call TogglePlayState
        targetImage.GetComponent<AspectRatioFitter>().aspectRatio = player.width / (float)player.height;
        backgroundRatioFitter.aspectRatio = player.width / (player.height * 1.05f);
    }

    public static IEnumerator GetThumbnail(PKT_MediaInfo mediaInfo, Action<Texture2D> onFinish)
    {
        Debug.Log($"Getting video thumbnail of media: {mediaInfo.name}");

        // enable video player but make it invisible
        _instance.canvasGroup.enabled = true;
        _instance.ToggleVisibility(true);

        var tempRenderTexture = new RenderTexture(_instance.player.targetTexture);
        var tempPlayer = new GameObject("GetThumbnail").AddComponent<VideoPlayer>();
        tempPlayer.renderMode = VideoRenderMode.RenderTexture;
        tempPlayer.targetTexture = tempRenderTexture;
        tempPlayer.audioOutputMode = VideoAudioOutputMode.None;

        if (!mediaInfo.PrepareVideo(tempPlayer))
        {
            Debug.LogWarning("Failed to prepare video.");
            _instance.ToggleVisibility(false);
            _instance.canvasGroup.enabled = false;
            onFinish?.Invoke(null);
        }

        tempPlayer.Play();
        while(!tempPlayer.isPlaying)
            yield return new WaitForEndOfFrame();

        Texture2D frameTexture = null;
        while(frameTexture == null || !tempRenderTexture.IsCreated())
        {
            Debug.Log($"Getting thumbnail for media: {mediaInfo.name}");
            yield return new WaitForSeconds(.1f);
            tempPlayer.Pause(); // Pause the video to capture the frame
            yield return new WaitForEndOfFrame();

            frameTexture = new Texture2D(tempRenderTexture.width, tempRenderTexture.height, TextureFormat.RGBA32, false);

            // Read pixels from the RenderTexture
            RenderTexture.active = tempRenderTexture;
            frameTexture.ReadPixels(new Rect(0, 0, tempRenderTexture.width, tempRenderTexture.height), 0, 0);
            frameTexture.Apply();
        }
        RenderTexture.active = null;

        tempPlayer.Stop();
        Destroy(tempRenderTexture);
        Destroy(tempPlayer.gameObject);

        _instance.ToggleVisibility(false);
        _instance.canvasGroup.enabled = false;

        onFinish?.Invoke(frameTexture);
    }

    private void RefreshBar()
    {
        if (!IsPlaying) return;
        playbackBar.UpdateProgress((float)(player.time / player.length));
    }

    public void TogglePlayState(bool state)
    {
        _isPlayingInternal = state;
        if (state)
        {
            if (!player.isPrepared) player.Prepare();
            else player.Play();
        } 
        else player.Pause();
    }

    public void SetVideoProgress(float progress)
    {
        if (!player.isPrepared) return;
        player.time = player.length * progress;
        _lastRefresh = Time.time;
    }

    public static void Load(PKT_MediaInfo info)
    {
        _instance.ToggleVisibility(true);
        _instance.title.text = string.IsNullOrWhiteSpace(info.name) ? "Video Player" : info.name;
        info.PrepareVideo(_instance.player);
    }

    public void CloseViewer()
    {
        player.Stop();
        player.clip = null;
        player.source = VideoSource.VideoClip;
        player.url = string.Empty;
        ToggleVisibility(false);
    }

    private void ToggleVisibility(bool visible)
    {
        //rootBackground.enabled = visible;
        rootPanel.SetActive(visible);
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
