using UnityEditor;
using UnityEngine;

namespace Molca
{
    public class LocalizationEditorUtility
    {
        [MenuItem("Molca/Localization/Refresh Text Style")]
        public static void RefreshTextStyle()
        {
            foreach (var lt in Object.FindObjectsByType<LocalizedText>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                lt.OnStyleRefresh();
        }
    }
}