using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum UserRole
{
    Manager, Operational, Guest
}
 
public enum Type
{
    Production, Utility
}

#region Plant Data
[Serializable]
public class PlantAttributes
{
    public string slug;
    public string name;
    public string uom;
    public object description;
    public int placeholder;
    public string sourceType;
    public APIValue value;
}

[Serializable]
public class APIPlantDatum
{
    public string type;
    public string id;
    public PlantAttributes attributes;
}

[Serializable]
public class Links
{
    public string self;
}

[Serializable]
public class APIPlant
{
    public Links links;
    public List<APIPlantDatum> data;
}

[Serializable]
public class APIValue
{
    public string value;
    public DateTime last_update;
}
#endregion

public class OverviewManager : MonoBehaviour
{
    public static OverviewManager instance;

    [HideInInspector] public List<MeshRenderer> allMachines;
    public MachineManager currentMachine;
    public POIButton currentPOI;

    [Header("General Attributes")]
    public Color dimmedColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    public Material transparanceMaterial;
    public GameObject directoryPanel;
    public GameObject nextIndicator;
    public GameObject textButton;

    [Header("Overview Attributes")]
    public Type currentType;
    public GameObject overviewUI;
    public GameObject overviewPOI;
    public Button prodButton;
    public Button utilButton;

    [Header("Parameter Panel")]
    public GameObject parameterParentPanel;
    public Button bottomParameterButton;
    public GameObject leftMonitoringPanel;
    public GameObject bottomMonitoringPanel;
    public bool leftParameterIsOpened;
    public bool bottomParameterIsOpened;
    public UnityEvent whenChooseProduction;
    public UnityEvent whenChooseUtility;

    void Awake()
    {
        instance = this;
        currentType = (Type)StaticData.type_id;
        allMachines = FindObjectsOfType<MeshRenderer>().ToList();

        if (currentType == Type.Production) prodButton.onClick.Invoke();
        else utilButton.onClick.Invoke();
    }

    void Start()
    {
        foreach (Transform item in transform)
        {
            item.gameObject.SetActive(true);
        }
    }

    public void ResetCurrentMachine()
    {
        if (currentMachine != null)
        {
            currentMachine.ResetAllMachines();
        }
    }

    public void ResetCurrentPOI()
    {
        if (currentPOI != null)
        {
            currentPOI.ClosePOIButton(true);
        }
    }

    public void SetOverviewPanel(bool state)
    {
        overviewUI.SetActive(state);
        overviewPOI.SetActive(state);
    }

    public void SetLeftParameterPanel(bool state)
    {
        if (!leftParameterIsOpened && state)
        {
            leftMonitoringPanel.GetComponent<Animator>().Play("Open");
            leftParameterIsOpened = true;
            SetBottomParameterPanel(false);
        }
        else if (leftParameterIsOpened && !state)
        {
            TrendHandler.instance.CloseTrendPanel();
            leftMonitoringPanel.GetComponent<Animator>().Play("Close");
            leftParameterIsOpened = false;
            currentPOI.ClosePOIButton(false);
        }

        bottomParameterButton.interactable = !leftParameterIsOpened;
    }

    public void SetBottomParameterPanel(bool state)
    {
        if (!bottomParameterIsOpened && state)
        {
            bottomMonitoringPanel.GetComponent<Animator>().Play("Open");
            bottomParameterIsOpened = true;
        }
        else if (bottomParameterIsOpened && !state)
        {
            bottomMonitoringPanel.GetComponent<Animator>().Play("Close");
            bottomParameterIsOpened = false;
        }
    }

    public void SetParameterPanel()
    {
        if (currentPOI == null) return;
        currentPOI.LeftButtonClicked(false);
    }

    public void ChangeType(int index)
    {
        StaticData.type_id = index;
        currentType = (Type)index;
        
        if (currentPOI != null)
        {
            currentPOI.ClosePOIButton(true);
        }

        //ResetCurrentMachine();
        //ResetCurrentPOI();

        if (currentType == Type.Production)
        {
            SetLeftParameterPanel(false);
            SetBottomParameterPanel(true);
            whenChooseProduction.Invoke();
        }
        else
        {
            SetBottomParameterPanel(false);
            whenChooseUtility.Invoke();
        }
    }

    public void SetupDirectoryButtons()
    {
        ResetDirectoryButtons();
        directoryPanel.SetActive(true);
        directoryPanel.transform.GetChild(0).
            GetComponentInChildren<TextMeshProUGUI>().
            text = ((Type)StaticData.type_id).ToString();

        foreach (var item in currentMachine.directoryTextButton)
        {
            var next = Instantiate(nextIndicator, directoryPanel.transform);
            var button = Instantiate(textButton, directoryPanel.transform);

            next.SetActive(true);
            button.SetActive(true);
            button.GetComponentInChildren<TextMeshProUGUI>().text = item.text;
            button.GetComponent<Button>().onClick.AddListener(delegate
            {
                item.eventClicked.Invoke();
            });
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(directoryPanel.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void ResetDirectoryButtons()
    {
        List<GameObject> list = new List<GameObject>();

        foreach (Transform child in directoryPanel.transform)
        {
            list.Add(child.gameObject);
        }

        for (int i = 3; i < list.Count; i++)
        {
            Destroy(list[i]);
        }

        directoryPanel.SetActive(false);
    }

    public void BackToMainMenu()
    {
        StaticData.need_login = false;
        StaticData.branchDetail = string.Empty;
        APIManager.instance.pendingRequests.Clear();
        SceneManager.LoadScene("Interface");
    }
}
