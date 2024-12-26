using UnityEngine;

public class HeightThresholdHandler : MonoBehaviour
{
    public Material targetMaterial; // Material yang menggunakan custom shader
    public float minHeight;    // Nilai minimum untuk _HeightThreshold
    public float maxHeight;   // Nilai maksimum untuk _HeightThreshold

    public void UpdateHeight(float value)
    {
        if (targetMaterial != null)
        {
            if (targetMaterial.HasProperty("_HeightThreshold"))
            {
                float mappedHeight = Mathf.Lerp(minHeight, maxHeight, value);
                targetMaterial.SetFloat("_HeightThreshold", mappedHeight);
            }
            else
            {
                Debug.LogWarning("Shader tidak memiliki properti '_HeightThreshold'");
            }
        }
        else
        {
            Debug.LogWarning("Target material belum diassign!");
        }
    }
}
