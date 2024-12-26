using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TextReference
{
    public string key;
    public TextMeshProUGUI text;
}

[Serializable]
public class ProgressImageReference
{
    public string key;
    public Image progressImage;
}

public class UIReference : MonoBehaviour
{
    public List<TextReference> textReferences;
    public List<ProgressImageReference> progressImageReferences;

    public TextMeshProUGUI FindText(string key)
    {
        foreach (var item in textReferences)
        {
            if (item.key == key)
                return item.text;
        }

        return null;
    }

    public Image FindProgressImage(string key)
    {
        foreach (var item in progressImageReferences)
        {
            if (item.key == key)
                return item.progressImage;
        }

        return null;
    }
}
