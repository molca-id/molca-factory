using ChartAndGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class TrendAttribute
{
    public string x;
    public int y;
}

[Serializable]
public class TrendDatum
{
    public string type;
    public TrendAttribute attributes;
}

[Serializable]
public class Trend
{
    public Links links;
    public List<TrendDatum> data;
}

[Serializable]
public class FilterTrendDatum
{
    public GameObject filterTab;
    public List<UserRole> allowedUsers;
}

public class TrendHandler : MonoBehaviour
{
    public static TrendHandler instance;
    public APITrendManager currentTrendManager;
    public GameObject trendPanel;
    public TextMeshProUGUI machineName;
    public GraphChart graph;
    public List<FilterTrendDatum> filterTrendData;

    void Awake()
    {
        instance = this;
    }

    public bool IsAllowed(List<UserRole> _allowedRoles)
    {
        foreach (var role in StaticData.current_user_data.role_id)
        {
            if (Enum.TryParse(role, true, out UserRole parsedRole))
            {
                if (_allowedRoles.Contains(parsedRole))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void ChangeTrendFilter(string filter)
    {
        StaticData.trend_filter = filter;
        currentTrendManager.ChooseTrend(true);
    }

    public void SetupTrends(APITrendManager manager, Trend data)
    {
        if (StaticData.trend_filter == "1day") graph.GetComponent<HorizontalAxis>().Format = AxisFormat.DateTime;
        else graph.GetComponent<HorizontalAxis>().Format = AxisFormat.Date;

        currentTrendManager = manager;
        machineName.text = currentTrendManager.title;

        graph.DataSource.StartBatch();
        graph.DataSource.ClearCategory("Player 1");
        graph.DataSource.ClearCategory("Player 2");

        foreach (var item in data.data)
        {
            graph.DataSource.AddPointToCategory(
                "Player 1",
                DateTime.Parse(item.attributes.x),
                Convert.ToDouble(item.attributes.y)
                );
        }

        foreach (var item in filterTrendData)
        {
            item.filterTab.SetActive(IsAllowed(item.allowedUsers));
        }

        graph.DataSource.EndBatch();
        trendPanel.SetActive(true);

        machineName.gameObject.SetActive(false);
        machineName.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(machineName.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void CloseTrendPanel()
    {
        if (currentTrendManager != null)
        {
            if (currentTrendManager.updateValueCoroutine != null)
            {
                currentTrendManager.StopCoroutine(currentTrendManager.updateValueCoroutine); // Hentikan coroutine dengan referensi
                currentTrendManager.updateValueCoroutine = null; // Set null untuk mencegah referensi zombie
            }
            currentTrendManager = null;
        }

        trendPanel.SetActive(false);
    }
}
