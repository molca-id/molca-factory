using System;
using UnityEngine;

[Serializable]
public struct BooleanColor
{
    public Color trueColor;
    public Color falseColor;

    public Color GetColor(bool value)
    {
        return value ? trueColor : falseColor;
    }
}