using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Data;


#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class RunStopRegenDatum
{
    public GameObject runIndicator;
    public GameObject stopIndicator;
    public GameObject regenIndicator;
}

[Serializable]
public class PassedNotPassedIndicator
{
    public GameObject passedIndicator;
    public GameObject notPassedIndicator;
}

[Serializable]
public class PassedNotPassedDatum
{
    public float minValue;
    public float maxValue;
    public bool isPassed;
}

[Serializable]
public class RangeTypeDatum
{
    public string value;
    public float minValue;
    public float maxValue;
    public UnityEvent whenFulfilled;
}

[Serializable]
public class EqualsToDatum
{
    public string value;
    public float targetValue;
    public UnityEvent whenFulfilled;
}

public enum ParameterType
{
    RunStopRegen,
    RangeValue,
    Progression,
    Default,
    EqualsTo,
    PassedNotPassed
}

public class ParameterHandler : MonoBehaviour
{
    public ParameterType type;
    [SerializeField] private List<UserRole> _allowedRoles = new List<UserRole>();
    [SerializeField] private string _parameterSlug;
    [SerializeField] private string _currentValue;

    public string parameterSlug
    {
        get
        {
            return _parameterSlug;
        }
        set
        {
            _parameterSlug = value;
        }
    }

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI valueText;
    public APITrendManager trendManager;
    public MonitorAttributes parameterDatum;
    [Space]
    public RunStopRegenDatum runStopRegenValue;
    [Space]
    public TextMeshProUGUI rangeValueText;
    public List<RangeTypeDatum> rangeValues = new List<RangeTypeDatum>();
    [Space]
    public Image progressionImage;
    public UnityEvent<float> whenProgressChanged;
    [Space]
    public TextMeshProUGUI equalsToText;
    public List<EqualsToDatum> equalsTo = new List<EqualsToDatum>();
    [Space]
    public TextMeshProUGUI passedNotPassedText;
    public List<PassedNotPassedDatum> passedNotPassedData;
    public PassedNotPassedIndicator passedNotPassedIndicator;

    public void SetParameterCard(MonitorAttributes datum)
    {
        parameterDatum = datum;
        _currentValue = datum.value.value;
        SetupValueText();

        switch (type)
        {
            case ParameterType.RunStopRegen:
                SetupRunStopRegen();
                break;
            case ParameterType.RangeValue:
                SetupRangeValue();
                break;
            case ParameterType.Progression:
                SetupProgression();
                break;
            case ParameterType.EqualsTo:
                SetupEqualsTo();
                break;
            case ParameterType.PassedNotPassed:
                SetupPassedNotPassed();
                break;
        }
    }

    public bool IsAllowed()
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

    private bool IsGuest()
    {
        List<string> roleIds = StaticData.current_user_data.role_id;
        if (roleIds.Count == 1)
        {
            if (Enum.TryParse(roleIds[0], true, out UserRole parsedRole))
            {
                if (parsedRole == UserRole.Guest)
                    return true;
            }
        }

        return false;
    }

    public void SetupValueText()
    {
        //if (titleText != null)
        //    titleText.text = parameterDatum.name;

        if (trendManager != null && !IsGuest())
        {
            if (!titleText.text.Contains("sprite"))
                titleText.text += "  <sprite=0>";

            GetComponent<Button>().interactable = true;
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(() => trendManager.ChooseTrend(true));
        }

        if (valueText == null)
            return;

        if (IsAllowed())
        {
            if (parameterDatum.uom == "duration")
            {
                valueText.text = $"{parameterDatum.value.value}";
            }
            else
            {
                try
                {
                    float parsedValue = float.Parse(parameterDatum.value.value);
                    if (parsedValue % 1 == 0) // Check if the value is an integer
                    {
                        valueText.text = (parameterDatum.uom != "unknown") ?
                            $"{(int)parsedValue} <size=65%>{parameterDatum.uom}" :
                            $"{(int)parsedValue}";
                    }
                    else
                    {
                        valueText.text = (parameterDatum.uom != "unknown") ?
                            $"{parsedValue.ToString("F2")} <size=65%>{parameterDatum.uom}" :
                            $"{parsedValue.ToString("F2")}";
                    }
                }
                catch
                {
                    valueText.text = "•••";
                }
            }
        }
        else
        {
            valueText.text = "•••";
        }
    }

    public void SetupRunStopRegen()
    {
        runStopRegenValue.runIndicator.SetActive(false);
        runStopRegenValue.stopIndicator.SetActive(false);
        runStopRegenValue.regenIndicator.SetActive(false);

        try
        {
            switch (parameterDatum.value.value.ToLower())
            {
                case "run":
                    runStopRegenValue.runIndicator.SetActive(true);
                    break;
                case "stop":
                    runStopRegenValue.stopIndicator.SetActive(true);
                    break;
                case "regenerasi":
                    runStopRegenValue.regenIndicator.SetActive(true);
                    break;
            }
        }
        catch
        {
            runStopRegenValue.stopIndicator.SetActive(true);
        }
    }

    public void SetupRangeValue()
    {
        try
        {
            float currentValue = float.Parse(parameterDatum.value.value);
            foreach (var range in rangeValues)
            {
                if (currentValue >= range.minValue &&
                    currentValue <= range.maxValue)
                {
                    if (IsAllowed())
                    {
                        if (rangeValueText != null)
                            rangeValueText.text = range.value;

                        if (valueText != null)
                            valueText.text = string.Empty;

                        range.whenFulfilled?.Invoke();
                    }
                    else
                    {
                        if (rangeValueText != null)
                            rangeValueText.text = "•••";

                        if (valueText != null)
                            valueText.text = "•••";
                    }

                    break;
                }
            }
        }
        catch
        {
            if (rangeValueText != null)
                rangeValueText.text = "•••";

            if (valueText != null)
                valueText.text = "•••";
        }
    }

    public void SetupProgression()
    {
        try
        {
            float currentValue = float.Parse(parameterDatum.value.value);
            progressionImage.fillAmount = currentValue / 100;

            if (IsAllowed())
            {
                if (currentValue % 1 == 0)
                {
                    valueText.text = $"{(int)currentValue}%";
                }
                else
                {
                    valueText.text = $"{currentValue.ToString("F2")}%";
                }
            }
            else
            {
                valueText.text = "•••";
            }
        }
        catch
        {
            progressionImage.fillAmount = 0 / 100;
            valueText.text = "•••";
        }

        whenProgressChanged?.Invoke(progressionImage.fillAmount);
    }

    public void SetupEqualsTo()
    {
        try
        {
            float currentValue = float.Parse(parameterDatum.value.value);
            foreach (var equal in equalsTo)
            {
                if (currentValue == equal.targetValue)
                {
                    if (IsAllowed())
                    {
                        if (equalsToText != null)
                        {
                            equalsToText.text = equal.value;
                        }

                        equal.whenFulfilled?.Invoke();
                    }
                    else
                    {
                        if (equalsToText != null)
                            equalsToText.text = "•••";

                        if (valueText != null)
                            valueText.text = "•••";
                    }

                    break;
                }
            }
        }
        catch
        {
            if (equalsToText != null)
                equalsToText.text = "•••";

            if (valueText != null)
                valueText.text = "•••";
        }
    }

    public void SetupPassedNotPassed()
    {
        if (IsAllowed())
        {
            float currentValue = float.Parse(parameterDatum.value.value);
            foreach (var item in passedNotPassedData)
            {
                if (currentValue >= item.minValue && 
                    currentValue <= item.maxValue)
                {
                    passedNotPassedIndicator.passedIndicator.SetActive(item.isPassed);
                    passedNotPassedIndicator.notPassedIndicator.SetActive(!item.isPassed);
                }
            }

            if (passedNotPassedText != null) 
                passedNotPassedText.text = "";
        }
        else
        {
            passedNotPassedIndicator.passedIndicator.SetActive(false);
            passedNotPassedIndicator.notPassedIndicator.SetActive(false);
            
            if (passedNotPassedText != null) 
                passedNotPassedText.text = "•••";
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ParameterHandler))]
public class ParameterHandlerEditor : Editor
{
    private void DrawProperty(SerializedProperty property, string label = null)
    {
        if (label != null)
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        else
            EditorGUILayout.PropertyField(property);
    }

    public override void OnInspectorGUI()
    {
        ParameterHandler handler = (ParameterHandler)target;

        // Serialized Properties
        var currentValueProp = serializedObject.FindProperty("_currentValue");
        var allowedRolesProp = serializedObject.FindProperty("_allowedRoles");
        var typeProp = serializedObject.FindProperty("type");
        var parameterSlugProp = serializedObject.FindProperty("_parameterSlug");
        var titleTextProp = serializedObject.FindProperty("titleText");
        var valueTextProp = serializedObject.FindProperty("valueText");
        var trendManagerProp = serializedObject.FindProperty("trendManager");
        var parameterDatumProp = serializedObject.FindProperty("parameterDatum");
        var runStopRegenValueProp = serializedObject.FindProperty("runStopRegenValue");
        var rangeValueTextProp = serializedObject.FindProperty("rangeValueText");
        var rangeValuesProp = serializedObject.FindProperty("rangeValues");
        var progressionImageProp = serializedObject.FindProperty("progressionImage");
        var whenProgressChangedProp = serializedObject.FindProperty("whenProgressChanged");
        var equalsToTextProp = serializedObject.FindProperty("equalsToText");
        var equalsToProp = serializedObject.FindProperty("equalsTo");
        var passedTextProp = serializedObject.FindProperty("passedNotPassedText");
        var passedDataProp = serializedObject.FindProperty("passedNotPassedData");
        var passedIndicator = serializedObject.FindProperty("passedNotPassedIndicator");

        // Draw Properties
        EditorGUI.BeginDisabledGroup(true);
        DrawProperty(currentValueProp, "Current Value");
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField("Allowed Roles", EditorStyles.boldLabel);

        if (allowedRolesProp != null && allowedRolesProp.isArray)
        {
            for (int i = 0; i < allowedRolesProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var roleProp = allowedRolesProp.GetArrayElementAtIndex(i);
                DrawProperty(roleProp, $"Role {i + 1}");

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    allowedRolesProp.DeleteArrayElementAtIndex(i);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("Add Role"))
        {
            allowedRolesProp.InsertArrayElementAtIndex(allowedRolesProp.arraySize);
        }

        EditorGUILayout.Space();

        DrawProperty(typeProp);
        DrawProperty(parameterSlugProp, "Parameter Slug");
        DrawProperty(titleTextProp);
        DrawProperty(valueTextProp);
        DrawProperty(trendManagerProp);
        DrawProperty(parameterDatumProp);

        EditorGUILayout.Space();

        switch ((ParameterType)typeProp.enumValueIndex)
        {
            case ParameterType.RunStopRegen:
                DrawProperty(runStopRegenValueProp.FindPropertyRelative("runIndicator"), "Run Indicator");
                DrawProperty(runStopRegenValueProp.FindPropertyRelative("stopIndicator"), "Stop Indicator");
                DrawProperty(runStopRegenValueProp.FindPropertyRelative("regenIndicator"), "Regen Indicator");
                break;

            case ParameterType.RangeValue:
                DrawProperty(rangeValueTextProp);
                DrawProperty(rangeValuesProp, "Range Values");
                break;

            case ParameterType.Progression:
                DrawProperty(progressionImageProp);
                DrawProperty(whenProgressChangedProp, "When Progress Changed");
                break;

            case ParameterType.EqualsTo:
                DrawProperty(equalsToTextProp);
                DrawProperty(equalsToProp, "Equals To");
                break;

            case ParameterType.PassedNotPassed:
                DrawProperty(passedTextProp);
                DrawProperty(passedDataProp);
                DrawProperty(passedIndicator);
                break;
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(handler);
    }
}
#endif
