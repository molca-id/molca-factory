using InteractiveViewer;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New ToolInfo", menuName = "PKT/Tool Info")]
public class PKT_ToolInfo : ScriptableObject
{
    public string id;
    public PKT_MediaInfo[] medias;

    [Header("Localization")]
    public PKT_DynamicLocalization localizedTitle;
    public PKT_DynamicLocalization localizedDetail;

    public string title => localizedTitle.String;
    public string detail => localizedDetail.String;

    public void Init()
    {
        localizedTitle.Init($"Tool-{id}_title");
        localizedDetail.Init($"Tool-{id}_detail");
    }
}
