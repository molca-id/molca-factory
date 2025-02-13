using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.HID;

[Serializable]
public class TextButton
{
    public string text;
    public UnityEvent eventClicked;
}

public class MachineManager : MonoBehaviour
{
    public float zoomFactor;
    public string machineName;
    public Transform pivotTarget;
    public Transform cameraTarget;
    public GameObject machinePOI;
    public GameObject highlightArea;
    public List<GameObject> hiddenMesh;
    public List<TextButton> directoryTextButton;
    
    Dictionary<MeshRenderer, Color> originalColors = new Dictionary<MeshRenderer, Color>();
    PivotCamera pivotCamera;
    Camera cam;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("Pivot Camera").GetComponent<Camera>();
        pivotCamera = cam.GetComponent<PivotCamera>();
    }

    public void SetupHiddenMesh(bool state)
    {
        foreach (var item in hiddenMesh)
        {
            item.SetActive(state);
        }
    }

    public void SetupHiddenMesh(bool state, int index)
    {
        if (index > hiddenMesh.Count - 1) return;
        hiddenMesh[index].SetActive(state);
    }

    public void FocusOnMachine(bool paramPanel)
    {
        if (machinePOI != null)
            machinePOI.SetActive(true);

        OverviewManager.instance.ResetCurrentMachine();
        OverviewManager.instance.currentMachine = this;
        OverviewManager.instance.SetOverviewPanel(false);
        OverviewManager.instance.SetupDirectoryButtons();

        StartCoroutine(pivotCamera.SmoothChangePivot(pivotTarget, zoomFactor, cameraTarget));
        DimAllMachines();

        if (paramPanel) return;
        OverviewManager.instance.SetLeftParameterPanel(paramPanel);
    }

    public void DimAllMachines()
    {
        if (highlightArea != null)
            highlightArea.SetActive(true);

        if (machinePOI != null)
            machinePOI.SetActive(true);

        SetLayerRecursively(
            gameObject, LayerMask.NameToLayer(StaticData.selected_machine_layer_name)
            );

        foreach (MeshRenderer meshRenderer in OverviewManager.instance.allMachines)
        {
            if (meshRenderer.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                if (!originalColors.ContainsKey(meshRenderer))
                {
                    // Periksa apakah shader memiliki properti "_Color"
                    if (meshRenderer.material.HasProperty("_Color"))
                    {
                        originalColors[meshRenderer] = meshRenderer.material.color;
                    }
                }
                meshRenderer.material.color = OverviewManager.instance.dimmedColor;
            }
        }

        SetupHiddenMesh(false);
    }

    public void ResetAllMachines()
    {
        if (highlightArea != null) 
            highlightArea.SetActive(false);

        if (machinePOI != null)
            machinePOI.SetActive(false);

        SetLayerRecursively(
            gameObject, LayerMask.NameToLayer("Default")
            );

        foreach (MeshRenderer meshRenderer in OverviewManager.instance.allMachines)
        {
            if (meshRenderer.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                if (originalColors.ContainsKey(meshRenderer))
                {
                    meshRenderer.material.color = originalColors[meshRenderer];
                }
            }
        }

        if (machinePOI != null)
            foreach (var item in machinePOI.GetComponentsInChildren<POIButton>())
                item.ClosePOIButton(false);

        originalColors.Clear();
        SetupHiddenMesh(true);
    }

    public void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}