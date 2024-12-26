using InteractiveViewer;

public interface IMediaHandler
{
    void Load(string url);
    void Load(PKT_MediaInfo info);
}