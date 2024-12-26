using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class APIImageManager : MonoBehaviour
{
    public string subDomain;
    public List<Image> images;

    private void Start()
    {
#if !UNITY_EDITOR
        APIManager.instance.QueueRequest(
            APIManager.instance.DownloadImageCoroutine(
                subDomain, res =>
                {
                    foreach (var item in images)
                    {
                        item.sprite = res;
                    }
                }));
#endif
    }
}
