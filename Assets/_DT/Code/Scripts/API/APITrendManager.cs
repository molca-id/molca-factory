using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APITrendManager : MonoBehaviour
{
    public string title;
    public string machineSlug;
    public string lineSlug;
    public string functionSlug;
    public string parameterSlug;
    public string subDomain;
    public GameObject loadingObj;
    public Trend trend;

    public Coroutine updateValueCoroutine; // Tambahkan variabel untuk menyimpan referensi coroutine

    public void ChooseTrend(bool initial)
    {
        if (initial)
        {
            if (updateValueCoroutine != null) // Hentikan coroutine yang berjalan jika ada
            {
                StopCoroutine(updateValueCoroutine);
            }

            updateValueCoroutine = StartCoroutine(UpdateValue());
            if (loadingObj != null) loadingObj.SetActive(true);
        }

        TrendHandler.instance.SetupTrends(this, trend);
    }

    public IEnumerator UpdateValue()
    {
        while (true)
        {
            var segments = new List<string>();

            if (!string.IsNullOrEmpty(machineSlug))
                segments.Add($"machines/{machineSlug}");
            if (!string.IsNullOrEmpty(lineSlug))
                segments.Add($"lines/{lineSlug}");
            if (!string.IsNullOrEmpty(functionSlug))
                segments.Add($"functions/{functionSlug}");
            if (!string.IsNullOrEmpty(parameterSlug))
                segments.Add($"parameters/{parameterSlug}");

            string basePath = string.Join("/", segments);
            subDomain = $"{basePath}/trends?filter_date={StaticData.trend_filter ?? ""}";

            APIManager.instance.QueueRequest(
                APIManager.instance.GetDataCoroutine(
                    subDomain, res =>
                    {
                        if (loadingObj != null) loadingObj.SetActive(false);
                        trend = JsonUtility.FromJson<Trend>(res);

                        if (TrendHandler.instance.currentTrendManager == this)
                            ChooseTrend(false);
                    }));

            yield return new WaitForSeconds(StaticData.cooldown_timer);
        }
    }
}
