using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ShortcutManager : MonoBehaviour
{
    public GameObject shortcutPanel;
    public TMP_InputField searchBar; 
    public Button machineResultButton;
    public List<MachineManager> machineManagers;

    private Transform buttonParent;

    void Start()
    {
        machineManagers = FindObjectsOfType<MachineManager>().ToList();
        searchBar.onValueChanged.AddListener(InstantiateMachineResult);
        buttonParent = machineResultButton.transform.parent;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            bool isActive = !shortcutPanel.activeSelf;
            shortcutPanel.SetActive(isActive);
            
            if (isActive)
            {
                searchBar = shortcutPanel.GetComponentInChildren<TMP_InputField>();
                if (searchBar != null)
                {
                    searchBar.Select();
                    searchBar.ActivateInputField();
                }
            }
            else
            {
                searchBar.text = "";
            }
        }       
    }

    private void InstantiateMachineResult(string searchText)
    {
        // Clear existing buttons
        foreach (Transform child in buttonParent)
        {
            if (child.gameObject != machineResultButton.gameObject)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            bool buttonFound = false;
            string searchLower = searchText.ToLower();

            foreach (var machine in machineManagers)
            {
                if (!machine.machineName.ToLower().Contains(searchLower)) continue;

                // Try to find existing button
                bool foundExisting = false;
                string machineName = machine.machineName;

                foreach (Transform child in buttonParent)
                {
                    var buttonText = child.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null && buttonText.text == machineName)
                    {
                        child.gameObject.SetActive(true);
                        foundExisting = true;
                        buttonFound = true;
                        break;
                    }
                }

                // Create new button if needed
                if (!foundExisting)
                {
                    var newButton = Instantiate(machineResultButton, buttonParent);
                    newButton.gameObject.SetActive(true);
                    buttonFound = true;

                    var buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = machineName;
                    }

                    newButton.onClick.AddListener(() => {
                        machine.FocusOnMachine(false);
                        shortcutPanel.SetActive(false);
                    });
                }
            }
            
            // Hide all if no matches
            if (!buttonFound)
            {
                foreach (Transform child in buttonParent)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        
        RefreshUIFitters(shortcutPanel.GetComponent<RectTransform>());
    }

    public static void RefreshUIFitters(RectTransform root)
    {
        if (root == null) return;

        var fitters = root.GetComponentsInChildren<ContentSizeFitter>(true);
        var groups = root.GetComponentsInChildren<LayoutGroup>(true);

        foreach (var fitter in fitters)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
        }

        foreach (var group in groups)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(group.GetComponent<RectTransform>());
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
    }
}
