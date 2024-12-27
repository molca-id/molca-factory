using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class POIButton : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText;
    public Transform leftPanel;
    public Image rightPanel;
    [Space]
    public APIParameterManager apiParamManager;
    public ParameterManager paramManager;
    public MachineManager focusOnMachine;
    public bool haveStatus;
    public bool isOpened;

    Camera cam;
    PivotCamera pivotCamera;
    UnityEngine.Color newColor;
    bool isRunning;

    void OnEnable()
    {
        cam = GameObject.FindGameObjectWithTag("Pivot Camera").GetComponent<Camera>();
        pivotCamera = cam.GetComponent<PivotCamera>();
        statusText.gameObject.SetActive(haveStatus);

        GetComponentInChildren<Animator>().Play("Close");
        StartCoroutine(UpdateScaleRoutine());
        SetupLeftRightButtons();
    }

    void SetupLeftRightButtons()
    {
        leftPanel.GetComponent<Button>().onClick.RemoveAllListeners();
        leftPanel.GetComponent<Button>().onClick.AddListener(() => LeftButtonClicked(true));

        if (focusOnMachine)
        {
            rightPanel.GetComponent<Button>().interactable = true;
            rightPanel.GetComponent<Button>().onClick.RemoveAllListeners();
            rightPanel.GetComponent<Button>().onClick.AddListener(RightButtonClicked);
        }
    }

    public void LeftButtonClicked(bool exception)
    {
        CloseAllPOIButton();
        OverviewManager.instance.currentPOI = this;

        if (isOpened)
        {
            GetComponentInChildren<Animator>().Play("Close");
            isOpened = false;
        }
        else
        {
            GetComponentInChildren<Animator>().Play("Open");
            isOpened = true;
        }

        if (!exception)
            return;

        if (paramManager != null)
        {
            foreach (Transform child in OverviewManager.instance.parameterParentPanel.transform)
                child.gameObject.SetActive(false);

            paramManager.gameObject.SetActive(isOpened);
            apiParamManager.gameObject.SetActive(isOpened);
            OverviewManager.instance.SetLeftParameterPanel(isOpened);
        }

        if (focusOnMachine != null)
        {
            focusOnMachine.SetupHiddenMesh(!isOpened);

            if (focusOnMachine.highlightArea != null)
            {
                focusOnMachine.highlightArea.SetActive(isOpened);
            }
        }
    }

    public void RightButtonClicked()
    {
        OverviewManager.instance.SetBottomParameterPanel(false);
        focusOnMachine.FocusOnMachine(true);
        ClosePOIButton(false);
    }

    public void ClosePOIButton(bool needHide)
    {
        transform.SetAsLastSibling();
        GetComponentInChildren<Animator>().Play("Close");
        isOpened = false;

        if (!needHide) 
            return;

        if (focusOnMachine != null)
        {
            focusOnMachine.SetupHiddenMesh(true, 0);

            if (focusOnMachine.highlightArea != null)
            {
                focusOnMachine.highlightArea.SetActive(false);
            }
        }
    }

    public void CloseAllPOIButton()
    {
        transform.SetAsLastSibling();
        foreach (var item in FindObjectsOfType<POIButton>().ToList())
        {
            if (item != this)
            {
                item.GetComponentInChildren<Animator>().Play("Close");
                item.isOpened = false;

                if (item.focusOnMachine != null)
                {
                    item.focusOnMachine.SetupHiddenMesh(true, 0);
                    
                    if (item.focusOnMachine.highlightArea != null)
                    {
                        item.focusOnMachine.highlightArea.SetActive(false);
                    }
                }
            }
        }
    }

    public void SetStatusText(bool status)
    {
        UnityEngine.Color color = UnityEngine.Color.white;

        if (status)
        {
            isRunning = true;
            statusText.text = "• Running";
            leftPanel.GetChild(0).gameObject.SetActive(true);
            leftPanel.GetChild(1).gameObject.SetActive(false);

            if (ColorUtility.TryParseHtmlString(StaticData.running_indicator, out color))
                statusText.color = color;
            if (ColorUtility.TryParseHtmlString(StaticData.right_panel_running, out color))
                rightPanel.color = color;
        }
        else
        {
            isRunning = false;
            statusText.text = "• Stop";
            statusText.color = color;
            leftPanel.GetChild(0).gameObject.SetActive(false);
            leftPanel.GetChild(1).gameObject.SetActive(true);

            if (ColorUtility.TryParseHtmlString(StaticData.right_panel_stop, out color))
                rightPanel.color = color;
        }
    }

    IEnumerator UpdateScaleRoutine()
    {
        while (true)
        {
            float t = Mathf.InverseLerp(
                pivotCamera.minZoomDistance, 
                pivotCamera.maxZoomDistance, 
                pivotCamera.currentZoomDistance);
            float scale = Mathf.Lerp(
                StaticData.minScale, 
                StaticData.maxScale, 
                t);

            transform.localScale = new Vector3(scale, scale, scale);
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                             cam.transform.rotation * Vector3.up);

            // change color card
            if (OverviewManager.instance != null &&
                isRunning)
            {
                if (OverviewManager.instance.currentType == Type.Production)
                {
                    if (ColorUtility.TryParseHtmlString(StaticData.production_poi, out newColor))
                        rightPanel.color = newColor;
                }
                else
                {
                    if (ColorUtility.TryParseHtmlString(StaticData.utility_poi, out newColor))
                        rightPanel.color = newColor;
                }
            }

            yield return null;
        }
    }
}
