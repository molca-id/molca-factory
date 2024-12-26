using UnityEngine;
using UnityEngine.UI;

namespace Molca.Utils
{
    [CreateAssetMenu(fileName = "Rect Utility", menuName = "Molca/Utils/Rect Utility")]
    public class RectUtility : ScriptableObject
    {
        public void ForceRebuildLayoutImmediate(RectTransform target)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(target);
        }
    }
}