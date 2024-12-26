using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using Molca;

namespace InteractiveViewer
{
    public class PKT_LocalizationManager : LocalizationManager
    {
        [SerializeField]
        private LocalizedString[] _sharedStrings;
        [SerializeField]
        private StringTable enTable;
        [SerializeField]
        private StringTable idTable;

        private static LocalizedString[] SharedStrings => (instance as PKT_LocalizationManager)._sharedStrings;
        private readonly string TABLE_KEY = "Dynamic";

        public override async void Initialize(Action<IRuntimeSubsystem> finishCallback)
        {
            await LocalizationSettings.InitializationOperation.Task;
            base.Initialize(finishCallback);
        }

        private void OnDestroy()
        {
            // Clear all dynamic table entries
            enTable.Clear();
            idTable.Clear();
        }

        public LocalizedString UpdateEntry(string key, string enValue, string idValue)
        {
            if (enTable.GetEntry(key) == null)
            {
                enTable.AddEntry(key, enValue);
                idTable.AddEntry(key, idValue);
            }
            else
            {
                enTable.GetEntry(key).Value = enValue;
                idTable.GetEntry(key).Value = idValue;
            }

            return GetLocale(key);
        }

        public LocalizedString GetLocale(string key)
        {
            return new LocalizedString(TABLE_KEY, key);
        }

        public static LocalizedString GetSharedString(string key)
        {
            for (int i = 0; i < SharedStrings.Length; i++)
            {
                if (SharedStrings[i].TableEntryReference.Key == key)
                    return SharedStrings[i];
            }
            Debug.LogError($"No shared string with key: {key}");
            return null;
        }
    }

    [Serializable]
    public class PKT_DynamicLocalization
    {
        public string en;
        public string id;

        public LocalizedString locale { get; private set; }
        public string String { get; private set; }//=> (locale == null || locale.IsEmpty) ? "" : locale.GetLocalizedString();

        public void Init(string key)
        {
            if(locale != null)
                locale.Clear();
            if(string.IsNullOrEmpty(key))
            {
                Debug.LogError($"Failed to initialize {this}, key can't be empty.");
                return;
            }

            if(string.IsNullOrEmpty(en) || string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"Failed to initialize {this} with key {key}, entry string can't be empty.");
                return;
            }
            locale = RuntimeManager.GetSubsystem<PKT_LocalizationManager>().UpdateEntry(key, en, id);
            locale.StringChanged += OnStringChanged;
            String = PKT_LocalizationManager.Language == PKT_LocalizationManager.ENGLISH ? en : id;
            Debug.Log($"Dynamic localization initialized with key {key}.\r\nString: {String}");
        }

        private void OnStringChanged(string value)
        {
            String = value;
        }
    }
}