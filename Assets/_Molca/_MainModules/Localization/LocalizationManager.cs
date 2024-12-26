using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Molca
{
    public class LocalizationManager : RuntimeSubsystem
    {
        protected static LocalizationManager instance;

        private HashSet<LocalizedText> _localizedTexts;

        private static string _currentLanguage;
        public static string Language => _currentLanguage;

        public const string PREF_LOCALE = "LOCALIZATION_LOCALE";
        public const string ENGLISH = "en";
        public const string INDONESIAN = "id";

        public override void Initialize(Action<IRuntimeSubsystem> finishCallback)
        {
            instance = this;
            _localizedTexts = new HashSet<LocalizedText>();
            SetLanguage(PlayerPrefs.GetString(PREF_LOCALE));
            LocalizationSettings.SelectedLocaleChanged += SetLanguageInternal;

            Activate();
            finishCallback?.Invoke(this);
        }

        private void Refresh()
        {
            //Debug.Log($"Refreshing, text count: {_localizedTexts.Count}");
            foreach (var txt in _localizedTexts)
                txt.OnRefresh(Language);
        }

        private void SetLanguageInternal(Locale locale)
        {
            _currentLanguage = locale.Identifier.Code;
            PlayerPrefs.SetString(PREF_LOCALE, _currentLanguage);
            instance.Refresh();
        }

        public static void SetLanguage(string lang)
        {
            lang = lang.Contains(ENGLISH, StringComparison.OrdinalIgnoreCase) ? ENGLISH : INDONESIAN;
            if (_currentLanguage == lang)
                return;

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(lang);
        }

        public static bool AddText(LocalizedText localizedText)
        {
            return instance._localizedTexts.Add(localizedText);
        }

        public static bool RemoveText(LocalizedText localizedText)
        {
            return instance._localizedTexts.Remove(localizedText);
        }
    }
}