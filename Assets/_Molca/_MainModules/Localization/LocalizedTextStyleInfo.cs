using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "New LTSI", menuName = "Molca/Localized Text Style Info")]
public class LocalizedTextStyleInfo : ScriptableObject
{
    public string id;
    public TMP_FontAsset font;
    public FontStyles style;
    public float minSize, maxSize, preferedSize;
}