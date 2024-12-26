using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class LengthIndicator : MonoBehaviour
{
    public float length;
    public float rectMultiplier;
    [SerializeField]
    private TextMeshProUGUI lengthText;
    [SerializeField]
    private string lengthUnit;

    private void Awake()
    {
        OnRefresh();
    }

    private void OnValidate()
    {
        OnRefresh();
    }

    private void OnRefresh()
    {
        var rtf = (transform as RectTransform);
        rtf.sizeDelta = new Vector2(length * rectMultiplier, rtf.sizeDelta.y);
        if(lengthText)
            lengthText.text = $"{length.ToString("F2")} {lengthUnit}";
    }
}
