using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ChartAndGraph;

public class PlanningManager : MonoBehaviour
{
    public int pageIndex;
    public Button prevButton;
    public Button nextButton;
    public APIParameterManager parameterManager;
    public List<TextMeshProUGUI> dateText;
    public List<PlanningHandler> planningHandlers;
    public List<MonitorPlanning> monitoringPlanning;

    [Header("Planning Trend")]
    public GraphChart graph;
    public GameObject graphPanel;
    public TextMeshProUGUI graphTitleText;

    string currentLine;

    public void SetupMonitoringPlanningData(List<MonitorPlanning> data)
    {
        monitoringPlanning = data;
    }

    public void SetupPageIndex(int index)
    {
        pageIndex += index;
        prevButton.interactable = true;
        nextButton.interactable = true;

        if (pageIndex == 0)
        {
            prevButton.interactable = false;
            nextButton.interactable = true;
        }
        else if (pageIndex == monitoringPlanning[0].data.Count - 1)
        {
            prevButton.interactable = true;
            nextButton.interactable = false;
        }

        SetupAllPlannings();
    }

    public void SetupAllPlannings()
    {
        for (int i = 0; i < dateText.Count; i++)
        {
            try
            {
                dateText[i].text = GetDateMonth(
                    monitoringPlanning[0].data[pageIndex].attributes[i].date
                    );
            }
            catch
            {
                dateText[i].text = "•••";
            }
        }

        foreach (var item in monitoringPlanning)
        {
            PlanningHandler handler = planningHandlers.
                Find(planning => planning.slug == item.slug);
            handler.SetupLine(item.data, pageIndex);
        }

        if (string.IsNullOrEmpty(currentLine)) return;
        OpenTrend(currentLine);
    }

    public void OpenTrend(string slug)
    {
        currentLine = slug;
        graphPanel.SetActive(true);

        graph.DataSource.StartBatch();
        graph.DataSource.ClearCategory("Player 1");
        graph.DataSource.ClearCategory("Player 2");

        foreach (var item in monitoringPlanning.Find(res => res.slug == slug).data)
        {
            graphTitleText.text = item.attributes[0].name;
            foreach (var datum in item.attributes)
            {
                if (!string.IsNullOrEmpty(datum.date)) 
                {
                    graph.DataSource.AddPointToCategory(
                        "Player 1",
                        DateTime.Parse(datum.date),
                        Convert.ToDouble(datum.value.target_production)
                        );

                    graph.DataSource.AddPointToCategory(
                        "Player 2",
                        DateTime.Parse(datum.date),
                        Convert.ToDouble(datum.value.actual_production)
                        ); 
                }
            }
        }

        graph.DataSource.EndBatch();
    }

    public void CloseTrend()
    {
        currentLine = string.Empty;
        graphPanel.SetActive(false);
    }

    public void ChangeUnitFilter(string filter)
    {
        StaticData.parameter_filter = filter;
        parameterManager.gameObject.SetActive(false);
        parameterManager.gameObject.SetActive(true);
        pageIndex = 0;
    }

    public string GetDateMonth(string dateString)
    {
        DateTime parsedDate = DateTime.Parse(dateString);
        return(parsedDate.ToString("dd/MM"));
    }

    public string GetDateMonthYear(string dateString)
    {
        DateTime parsedDate = DateTime.Parse(dateString);
        return (parsedDate.ToString("dd/MM/yyyy"));
    }
}
