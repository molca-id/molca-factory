using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ImageColorDatum
{
    public float minValue;
    public float maxValue;
    public Color color;
}

public class ImageColorHandler : MonoBehaviour
{
    public float minValue;
    public float maxValue;
    public List<Image> images;
    public List<ImageColorDatum> imageColorData;

    public void SetupImageColor(float value)
    {
        // Transform the value from range [0, 1] to [minValue, maxValue]
        float transformedValue = (value * (maxValue - minValue)) + minValue;

        foreach (var item in imageColorData)
        {
            if (transformedValue <= item.maxValue && transformedValue >= item.minValue)
            {
                foreach (var image in images)
                {
                    image.color = item.color;
                }
            }
        }
    }
}
