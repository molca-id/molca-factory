using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.Globalization;
using System;

[Serializable]
public class ROCEData
{
    [Header("Shopfloor Data")]
    [Range(1, 3)]
    public int shiftNumberPerDay;
    [Range(1, 8)]
    public int workingHourPerShift;
    [Range(0, 100)]
    public float availabilityPercentage;
    [Range(0, 100)]
    public float performancePercentage;
    [Range(0, 100)]
    public float qualityPercentage;
    public long cycleTime;

    [Header("Finance Data")]
    public long pricePerUnit;
    public long variableCostPerUnit;
    public long directMaintenanceCostYearly;
    public long deprecationYearly;
    public long otherFixedCost;

    [Header("Capital Investment Data")]
    public long fixedAssets;
    public long NOWC;
}

public class ROCEManager : MonoBehaviour
{
    public ROCEDataScriptableObject roceData;
    public RectTransform rocePanel;
    public ROCEData roceImprovementData;

    [Header("Result UI")]
    public TextMeshProUGUI profitValueBeforeText;
    public TextMeshProUGUI revenueValueBeforeText;
    public TextMeshProUGUI roceValueBeforeText;
    public TextMeshProUGUI profitValueAfterText;
    public TextMeshProUGUI revenueValueAfterText;
    public TextMeshProUGUI roceValueAfterText;

    [Header("Shopfloor UI")]
    public Slider shiftNumberPerDay;
    public Slider workingHourPerShift;
    public TextMeshProUGUI shiftNumberPerDayText;
    public TextMeshProUGUI workingHourPerShiftText;
    public TMP_InputField availabilityPercentage;
    public TMP_InputField performancePercentage;
    public TMP_InputField qualityPercentage;
    public TMP_InputField cycleTime;

    [Header("Finance UI")]
    public TMP_InputField pricePerUnit;
    public TMP_InputField variableCostPerUnit;
    public TMP_InputField directMaintenanceCostYearly;
    public TMP_InputField deprecationYearly;
    public TMP_InputField otherFixedCost;

    [Header("Capital Investment UI")]
    public TMP_InputField fixedAssets;
    public TMP_InputField NOWC;

    void Start()
    {
        RandomizeROCEData();
        RefreshUIFitters(rocePanel);
        StartCoroutine(UpdateRoutine());
    }

    private IEnumerator UpdateRoutine()
    {
        while (true)
        {
            SetupROCEImprovementData();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void RandomizeROCEData()
    {
        // Randomize shopfloor data within valid ranges
        //roceData.shiftNumberPerDay = Random.Range(1, 4);
        //roceData.workingHourPerShift = Random.Range(1, 4); 
        //roceData.availabilityPercentage = Random.Range(0f, 100f);
        //roceData.performancePercentage = Random.Range(0f, 100f);
        //roceData.qualityPercentage = Random.Range(0f, 100f);
        //roceData.cycleTime = (int)Random.Range(0f, 99999f);
//
        //// Randomize finance data with reasonable ranges
        //roceData.pricePerUnit = (long)Random.Range(100000f, 1000000f);
        //roceData.variableCostPerUnit = (long)Random.Range(50000f, 500000f);
        //roceData.directMaintenanceCostYearly = (long)Random.Range(10000000f, 100000000f);
        //roceData.deprecationYearly = (long)Random.Range(50000000f, 500000000f);
        //roceData.otherFixedCost = (long)Random.Range(10000000f, 100000000f);
//
        //// Randomize capital investment
        //roceData.fixedAssets = (long)Random.Range(1000000000f, 10000000000f);
        //roceData.NOWC = (long)Random.Range(100000000f, 1000000000f);

        // Assign shopfloor data to UI
        shiftNumberPerDay.value = roceData.roceData.shiftNumberPerDay;
        workingHourPerShift.value = roceData.roceData.workingHourPerShift;
        availabilityPercentage.text = roceData.roceData.availabilityPercentage.ToString("F2");
        performancePercentage.text = roceData.roceData.performancePercentage.ToString("F2"); 
        qualityPercentage.text = roceData.roceData.qualityPercentage.ToString("F2");
        cycleTime.text = roceData.roceData.cycleTime.ToString();

        // Assign finance data to UI using Indonesian number format
        pricePerUnit.text = roceData.roceData.pricePerUnit.ToString("N0", new CultureInfo("id-ID"));
        variableCostPerUnit.text = roceData.roceData.variableCostPerUnit.ToString("N0", new CultureInfo("id-ID"));
        directMaintenanceCostYearly.text = roceData.roceData.directMaintenanceCostYearly.ToString("N0", new CultureInfo("id-ID"));
        deprecationYearly.text = roceData.roceData.deprecationYearly.ToString("N0", new CultureInfo("id-ID"));
        otherFixedCost.text = roceData.roceData.otherFixedCost.ToString("N0", new CultureInfo("id-ID"));

        // Assign capital investment data to UI
        fixedAssets.text = roceData.roceData.fixedAssets.ToString("N0", new CultureInfo("id-ID"));
        NOWC.text = roceData.roceData.NOWC.ToString("N0", new CultureInfo("id-ID"));
        
        // Assign calculation results to before UIs
        profitValueBeforeText.text = "IDR " + CalculateProfit(roceData.roceData).ToString("N0", new CultureInfo("id-ID"));
        revenueValueBeforeText.text = "IDR " + CalculateRevenues(roceData.roceData).ToString("N0", new CultureInfo("id-ID")); 
        roceValueBeforeText.text = CalculateROCE(roceData.roceData).ToString("F2") + "%";

        // Deep copy instead of shallow copy
        roceImprovementData = new ROCEData
        {
            shiftNumberPerDay = roceData.roceData.shiftNumberPerDay,
            workingHourPerShift = roceData.roceData.workingHourPerShift,
            availabilityPercentage = roceData.roceData.availabilityPercentage,
            performancePercentage = roceData.roceData.performancePercentage,
            qualityPercentage = roceData.roceData.qualityPercentage,
            cycleTime = roceData.roceData.cycleTime,
            pricePerUnit = roceData.roceData.pricePerUnit,
            variableCostPerUnit = roceData.roceData.variableCostPerUnit,
            directMaintenanceCostYearly = roceData.roceData.directMaintenanceCostYearly,
            deprecationYearly = roceData.roceData.deprecationYearly,
            otherFixedCost = roceData.roceData.otherFixedCost,
            fixedAssets = roceData.roceData.fixedAssets,
            NOWC = roceData.roceData.NOWC
        };
    }

    private void SetupROCEImprovementData()
    {
        try
        {
            // Update shopfloor data from UI
            roceImprovementData.shiftNumberPerDay = (int)shiftNumberPerDay.value;
            roceImprovementData.workingHourPerShift = (int)workingHourPerShift.value;
            
            // Parse percentage inputs with error handling
            if (!string.IsNullOrEmpty(availabilityPercentage.text))
                roceImprovementData.availabilityPercentage = float.Parse(availabilityPercentage.text);
            if (!string.IsNullOrEmpty(performancePercentage.text))
                roceImprovementData.performancePercentage = float.Parse(performancePercentage.text);
            if (!string.IsNullOrEmpty(qualityPercentage.text))
                roceImprovementData.qualityPercentage = float.Parse(qualityPercentage.text);
            if (!string.IsNullOrEmpty(cycleTime.text))
                roceImprovementData.cycleTime = long.Parse(cycleTime.text.Replace(".", ""));

            // Parse finance data (remove formatting first)
            if (!string.IsNullOrEmpty(pricePerUnit.text))
                roceImprovementData.pricePerUnit = long.Parse(pricePerUnit.text.Replace(".", ""));
            if (!string.IsNullOrEmpty(variableCostPerUnit.text))
                roceImprovementData.variableCostPerUnit = long.Parse(variableCostPerUnit.text.Replace(".", ""));
            if (!string.IsNullOrEmpty(directMaintenanceCostYearly.text))
                roceImprovementData.directMaintenanceCostYearly = long.Parse(directMaintenanceCostYearly.text.Replace(".", ""));
            if (!string.IsNullOrEmpty(deprecationYearly.text))
                roceImprovementData.deprecationYearly = long.Parse(deprecationYearly.text.Replace(".", ""));
            if (!string.IsNullOrEmpty(otherFixedCost.text))
                roceImprovementData.otherFixedCost = long.Parse(otherFixedCost.text.Replace(".", ""));

            // Parse capital investment data
            if (!string.IsNullOrEmpty(fixedAssets.text))
                roceImprovementData.fixedAssets = long.Parse(fixedAssets.text.Replace(".", ""));
            if (!string.IsNullOrEmpty(NOWC.text))
                roceImprovementData.NOWC = long.Parse(NOWC.text.Replace(".", ""));

            shiftNumberPerDayText.text = roceImprovementData.shiftNumberPerDay.ToString();
            workingHourPerShiftText.text = roceImprovementData.workingHourPerShift.ToString();

            // Update result texts
            profitValueAfterText.text = "IDR " + CalculateProfit(roceImprovementData).ToString("N0", new CultureInfo("id-ID"));
            revenueValueAfterText.text = "IDR " + CalculateRevenues(roceImprovementData).ToString("N0", new CultureInfo("id-ID")); 
            roceValueAfterText.text = CalculateROCE(roceImprovementData).ToString("F2") + "%";
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error updating ROCE improvement data: " + e.Message);
        }
    }

    #region ROCE Calculations
    private long CalculateProductionPlanPerYear(ROCEData data)
    {
        long result = data.shiftNumberPerDay * 
               data.workingHourPerShift * 
               data.cycleTime *
               365;
        return result;
    }

    private float CalculateOEE(ROCEData data)
    {
        float result = (data.availabilityPercentage / 100f) * 
               (data.performancePercentage / 100f) * 
               (data.qualityPercentage / 100f);
        return result;
    }

    private long CalculateActualProductionPerYear(ROCEData data)
    {
        long result = (long)(CalculateProductionPlanPerYear(data) * CalculateOEE(data));
        return result;
    }

    private long CalculateContributionMarginPerUnit(ROCEData data)
    {
        long result = data.pricePerUnit - data.variableCostPerUnit;
        return result;
    }

    private long CalculateContributionMarginPerYear(ROCEData data)
    {
        long result = (long)(CalculateActualProductionPerYear(data) * CalculateContributionMarginPerUnit(data));
        return result;
    }

    private long CalculateFixedCostPerYear(ROCEData data)
    {
        long result = data.directMaintenanceCostYearly + 
               data.deprecationYearly + 
               data.otherFixedCost;
        return result;
    }

    private long CalculateProfit(ROCEData data)
    {
        long result = CalculateContributionMarginPerYear(data) - CalculateFixedCostPerYear(data);
        return result;
    }

    private long CalculateRevenues(ROCEData data)
    {
        long result = (long)(data.pricePerUnit * CalculateActualProductionPerYear(data));
        return result;
    }

    private long CalculateCapitalEmployed(ROCEData data)
    {
        long result = data.fixedAssets + data.NOWC;
        return result;
    }

    private float CalculateProfitability(ROCEData data)
    {
        float result = (float)CalculateProfit(data) / CalculateRevenues(data);
        return result;
    }

    private float CalculateCapitalTurnoverRate(ROCEData data)
    {
        float result = (float)CalculateRevenues(data) / CalculateCapitalEmployed(data);
        return result;
    }

    private float CalculateROCE(ROCEData data)
    {
        float result = 100f * CalculateProfitability(data) * CalculateCapitalTurnoverRate(data);
        return result;
    }
    #endregion

    public static void RefreshUIFitters(RectTransform root)
    {
        if (root == null) return;

        // Get all ContentSizeFitters and force them to refresh
        foreach (ContentSizeFitter fitter in root.GetComponentsInChildren<ContentSizeFitter>(true))
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
        }

        // Refresh LayoutGroups
        foreach (LayoutGroup layoutGroup in root.GetComponentsInChildren<LayoutGroup>(true))
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }

        // Final refresh on the root itself
        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
    }
}