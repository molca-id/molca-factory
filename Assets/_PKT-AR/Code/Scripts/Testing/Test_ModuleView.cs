using System.Collections;
using UnityEngine;
using Molca;
using InteractiveViewer;
using NaughtyAttributes;
using TMPro;

/// <summary>
/// This class need authentication
/// </summary>
public class Test_ModuleView : MonoBehaviour
{
    [InfoBox("Module will only be assigned after user authenticated.")]
    public PKT_ModuleInfo moduleInfo;
    public TextMeshProUGUI touchState;

    private CameraController _camController;

    private IEnumerator Start()
    {
        if (touchState != null)
            InvokeRepeating(nameof(CheckFPS), 1f, .4f);

        _camController = FindFirstObjectByType<CameraController>();

        yield return new WaitUntil(RuntimeManager.IsReady);

        if(GameManager.ActiveModule == null)
            GameManager.ActiveModule = moduleInfo;
    }

    private void CheckFPS()
    {
        touchState.SetText($"{(1f / Time.deltaTime):F1} FPS");
    }
}
