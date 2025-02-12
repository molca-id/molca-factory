using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(fileName = "ROCE Data", menuName = "ScriptableObjects/ROCE Current Data", order = 1)]
public class ROCEDataScriptableObject : ScriptableObject
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
    public int cycleTime;

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