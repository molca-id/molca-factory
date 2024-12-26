using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlanningHandler : MonoBehaviour
{
    public string slug;
    public PlanningTableHandler totalTableHandler;
    public List<PlanningTableHandler> planningTableHandlers;

    public void SetupLine(List<MonitorPlanningDatum> data, int index)
    {
        for (int i = 0; i < data[index].attributes.Count; i++)
        {
            MonitorAttributes datum = data[index].attributes[i];
            planningTableHandlers[i].SetupTable(
                datum.value.actual_production,
                datum.value.target_production
                );
        }

        double actual = 0;
        double target = 0;
        foreach (MonitorPlanningDatum datum in data)
        {
            foreach (var attribute in datum.attributes)
            {
                try
                {
                    actual += Convert.ToInt32(attribute.value.actual_production);
                    target += Convert.ToInt32(attribute.value.target_production);
                }
                catch
                {
                    continue;
                }
            }
        }

        totalTableHandler.SetupTable(
            actual.ToString(),
            target.ToString()
            );
    }
}
