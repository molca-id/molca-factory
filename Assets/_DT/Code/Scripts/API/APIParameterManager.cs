using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APIParameterManager : MonoBehaviour
{
    public string subDomain;
    public string filterName;
    public List<GameObject> loadingObjs;
    public List<ParameterManager> parameterManagers;
    [TextArea] public string response;

    void OnEnable()
    {
        SetupLoadingObjects(true);
        StartCoroutine(UpdateValue());
    }

    void OnDisable()
    {
        StopCoroutine(UpdateValue());
    }

    void SetupLoadingObjects(bool state)
    {
        foreach (var item in loadingObjs)
        {
            item.SetActive(state);
        }
    }

    IEnumerator UpdateValue()
    {
        while (true)
        {
            string temp = string.IsNullOrEmpty(filterName) ? 
                subDomain : $"{subDomain}?{filterName}={StaticData.parameter_filter}";

            APIManager.instance.QueueRequest(
                APIManager.instance.GetDataCoroutine(
                    temp, res =>
                    {
                        SetupLoadingObjects(false);
                        foreach (var item in parameterManagers)
                        {
                            if (!string.IsNullOrEmpty(response))
                            {
                                item.Setup(response);
                            }
                            else
                            {
                                item.Setup(res);
                            }
                        }
                    }));

            yield return new WaitForSeconds(StaticData.cooldown_timer);
        }
    }
}
