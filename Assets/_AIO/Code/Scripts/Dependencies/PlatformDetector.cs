using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;

public class PlatformDetector : MonoBehaviour
{
    public static PlatformDetector instance;

    public bool onDevelopment;
    public bool isMobilePlatform;

    [DllImport("__Internal")]
    private static extern bool IsMobile();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        isMobilePlatform = IsMobile();
#endif

        if (onDevelopment)
        {
            isMobilePlatform = true;
            return;
        }

        if (isMobilePlatform) StaticData.is_mobile = true;
        else StaticData.is_mobile = false;
    }
}