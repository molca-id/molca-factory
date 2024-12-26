using UnityEngine;
using UnityEngine.Localization;
using Molca;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Linq;
using System.Collections;

namespace InteractiveViewer
{
    [DisallowMultipleComponent]
    public class PKT_LocalizedText : LocalizedText
    {
        [SerializeField]
        private LocalizedString _localizedString;

        public override void OnRefresh(string lang)
        {
            if (_localizedString == null || _localizedString.IsEmpty)
                return;

            if(!CheckStringValidity(_localizedString))
            {
                Debug.LogError($"Invalid localized string for {GetObjectHieararchy()}");
                return;
            }

            IEnumerator coroutineInternal()
            {
                var async = _localizedString.GetLocalizedStringAsync();
                while(!async.IsDone) yield return new WaitForEndOfFrame();
                text = async.Result;
                LayoutRebuilder.ForceRebuildLayoutImmediate(_tmpText.rectTransform);
            }
            StartCoroutine(coroutineInternal());
        }

        public void SetLocalizedString(LocalizedString localizedString)
        {
            text = ""; // Empty text for pooled items
            _localizedString = localizedString;
            OnRefresh(LocalizationManager.Language);
        }

        public bool CheckStringValidity(LocalizedString localizedString)
        {
            // Check if the string table exists
            var stringTable = LocalizationSettings.StringDatabase.GetTable(localizedString.TableReference);

            if (stringTable == null)
            {
                Debug.LogWarning("String table does not exist");
                return false;
            }

            // Check if the specific entry exists
            return stringTable.ContainsKey(localizedString.TableEntryReference);
        }

        private string GetObjectHieararchy()
        {
            return string.Join("/",
                transform.GetComponentsInParent<Transform>()
                .Reverse()
                .Select(t => t.name)
                .ToArray());
        }
    }
}