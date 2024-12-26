using TMPro;

namespace Molca
{
    public static class LocalizationExtension
    {
        public static bool TryGetLocalizedText(this TextMeshProUGUI text, out LocalizedText localizedText)
        {
            localizedText = text.GetComponent<LocalizedText>();
            return localizedText != null;
        }
    }
}