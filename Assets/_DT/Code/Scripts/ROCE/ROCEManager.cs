using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.Globalization;

public class ROCEManager : MonoBehaviour
{
    public ROCEDataScriptableObject roceData;
    public RectTransform rocePanel;

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
            // Assign calculation results to before UIs
            profitValueBeforeText.text = "IDR " + CalculateProfit(roceData).ToString("N0", new CultureInfo("id-ID"));
            revenueValueBeforeText.text = "IDR " + CalculateRevenues(roceData).ToString("N0", new CultureInfo("id-ID")); 
            roceValueBeforeText.text = CalculateROCE(roceData).ToString("F2") + "%";

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
        shiftNumberPerDay.value = roceData.shiftNumberPerDay;
        workingHourPerShift.value = roceData.workingHourPerShift;
        availabilityPercentage.text = roceData.availabilityPercentage.ToString("F2");
        performancePercentage.text = roceData.performancePercentage.ToString("F2"); 
        qualityPercentage.text = roceData.qualityPercentage.ToString("F2");
        cycleTime.text = roceData.cycleTime.ToString();

        // Assign finance data to UI using Indonesian number format
        pricePerUnit.text = roceData.pricePerUnit.ToString("N0", new CultureInfo("id-ID"));
        variableCostPerUnit.text = roceData.variableCostPerUnit.ToString("N0", new CultureInfo("id-ID"));
        directMaintenanceCostYearly.text = roceData.directMaintenanceCostYearly.ToString("N0", new CultureInfo("id-ID"));
        deprecationYearly.text = roceData.deprecationYearly.ToString("N0", new CultureInfo("id-ID"));
        otherFixedCost.text = roceData.otherFixedCost.ToString("N0", new CultureInfo("id-ID"));

        // Assign capital investment data to UI
        fixedAssets.text = roceData.fixedAssets.ToString("N0", new CultureInfo("id-ID"));
        NOWC.text = roceData.NOWC.ToString("N0", new CultureInfo("id-ID"));
    }

    #region ROCE Calculations
    private long CalculateProductionPlanPerYear(ROCEDataScriptableObject data)
    {
        long result = data.shiftNumberPerDay * 
               data.workingHourPerShift * 
               data.cycleTime *
               365;
        return result;
    }

    private float CalculateOEE(ROCEDataScriptableObject data)
    {
        float result = (data.availabilityPercentage / 100f) * 
               (data.performancePercentage / 100f) * 
               (data.qualityPercentage / 100f);
        return result;
    }

    private long CalculateActualProductionPerYear(ROCEDataScriptableObject data)
    {
        long result = (long)(CalculateProductionPlanPerYear(data) * CalculateOEE(data));
        return result;
    }

    private long CalculateContributionMarginPerUnit(ROCEDataScriptableObject data)
    {
        long result = data.pricePerUnit - data.variableCostPerUnit;
        return result;
    }

    private long CalculateContributionMarginPerYear(ROCEDataScriptableObject data)
    {
        long result = (long)(CalculateActualProductionPerYear(data) * CalculateContributionMarginPerUnit(data));
        return result;
    }

    private long CalculateFixedCostPerYear(ROCEDataScriptableObject data)
    {
        long result = data.directMaintenanceCostYearly + 
               data.deprecationYearly + 
               data.otherFixedCost;
        return result;
    }

    private long CalculateProfit(ROCEDataScriptableObject data)
    {
        long result = CalculateContributionMarginPerYear(data) - CalculateFixedCostPerYear(data);
        return result;
    }

    private long CalculateRevenues(ROCEDataScriptableObject data)
    {
        long result = (long)(data.pricePerUnit * CalculateActualProductionPerYear(data));
        return result;
    }

    private long CalculateCapitalEmployed(ROCEDataScriptableObject data)
    {
        long result = data.fixedAssets + data.NOWC;
        return result;
    }

    private float CalculateProfitability(ROCEDataScriptableObject data)
    {
        float result = (float)CalculateProfit(data) / CalculateRevenues(data);
        return result;
    }

    private float CalculateCapitalTurnoverRate(ROCEDataScriptableObject data)
    {
        float result = (float)CalculateRevenues(data) / CalculateCapitalEmployed(data);
        return result;
    }

    private float CalculateROCE(ROCEDataScriptableObject data)
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