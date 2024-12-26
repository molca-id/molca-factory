using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Molca
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public abstract class LocalizedText : MonoBehaviour
    {
        //public string pathId;
        public LocalizedTextStyleInfo styleInfo;

        protected TextMeshProUGUI _tmpText;

        protected string text
        {
            get => (_tmpText ??= GetComponent<TextMeshProUGUI>()).text;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => (_tmpText ??= GetComponent<TextMeshProUGUI>()).SetText(value);
        }

        protected virtual async void OnEnable()
        {
            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            if (_tmpText != null)
                _tmpText = GetComponent<TextMeshProUGUI>();
            yield return new WaitUntil(RuntimeManager.IsReady);

            LocalizationManager.AddText(this);
            OnRefresh(LocalizationManager.Language);
        }

        protected virtual void OnDisable()
        {
            LocalizationManager.RemoveText(this);
        }

        private void OnValidate()
        {
            OnStyleRefresh();
        }

        public abstract void OnRefresh(string lang);
        public virtual void OnStyleRefresh()
        {
            if (!styleInfo)
                return;

            TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
            text.font = styleInfo.font;
            text.fontStyle = styleInfo.style;
            text.fontSize = styleInfo.preferedSize;
            text.fontSizeMin = styleInfo.minSize;
            text.fontSizeMax = styleInfo.maxSize;
        }
    }
}