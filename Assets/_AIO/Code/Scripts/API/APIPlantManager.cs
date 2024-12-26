using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class APIPlantManager : MonoBehaviour
{
    public string subDomain;
    public GameObject loadingObj;
    public List<POIButton> buttons;
    public APIPlant plant;

    void Start()
    {
        if (loadingObj != null) loadingObj.SetActive(true);
        StartCoroutine(UpdateValue());
    }

    void OnDisable()
    {
        StopCoroutine(UpdateValue());
    }

    void SetupData()
    {
        foreach (var item in buttons)
        {
            APIPlantDatum datum = plant.data.Find(
                res => res.attributes.placeholder == buttons.IndexOf(item) + 1
                );

            if (datum != null)
            {
                item.nameText.text = datum.attributes.name;
                item.name = datum.attributes.name;

                int value = Convert.ToInt32(datum.attributes.value.value);
                if (value == 0) item.SetStatusText(false);
                else item.SetStatusText(true);
            }
            else
            {
                item.nameText.text = "Loading...";
                item.SetStatusText(false);
            }
        }
    }

    IEnumerator UpdateValue()
    {
        while (true)
        {
            APIManager.instance.QueueRequest(
                APIManager.instance.GetDataCoroutine(
                    subDomain, res =>
                    {
                        if (loadingObj != null) loadingObj.SetActive(false);
                        plant = JsonUtility.FromJson<APIPlant>(res);
                        SetupData();
                    }));

            yield return new WaitForSeconds(StaticData.cooldown_timer);
        }
    }
}
