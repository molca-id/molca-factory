using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleLocalization.Scripts;

[Serializable]
public class LanguageDatum
{
    public string key;
    public GameObject checkmark;
    public GameObject selected;
}

public class LanguageHandler : MonoBehaviour
{
    public GameObject languageOption;
    public List<LanguageDatum> languageSelection;

    public void Awake()
    {
        InitLocalization();
    }

    public void InitLocalization()
    {
        string lang = string.Empty;
        lang = PlayerPrefs.GetString("Language");
        if (string.IsNullOrEmpty(lang)) lang = "id-ID";
        SetLocalization(lang);
    }

    public void SetLocalization(string localization)
    {
        LocalizationManager.Read();
        LocalizationManager.Language = localization;
        PlayerPrefs.SetString("Language", localization);

        languageOption.SetActive(false);
        languageSelection.ForEach(t =>
        {
            t.selected.SetActive(false);
            t.checkmark.SetActive(false);
        });

        var selected = languageSelection.Find(lang => lang.key == localization);
        selected.selected.SetActive(true);
        selected.checkmark.SetActive(true);
    }
}
