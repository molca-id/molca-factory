using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class VersionText : MonoBehaviour
{
    void Start()
    {
        GetComponent<TextMeshProUGUI>().SetText($"version. {Application.version}");
        Destroy(this);
    }
}
