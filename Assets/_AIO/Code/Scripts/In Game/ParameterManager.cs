using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using TMPro;
using UnityEngine;

[Serializable]
public class MonitorAttributes
{
    public string date;
    public string slug;
    public string name;
    public string uom;
    public string description;
    public string sourceType;
    public MonitorValue value;
}

[Serializable]
public class MonitorValue
{
    public string value;
    public string actual_production;
    public string target_production;
    public string last_update;
}

[Serializable]
public class MonitorPlanning
{
    public string slug;
    public List<MonitorPlanningDatum> data;
}

[Serializable]
public class MonitorPlanningDatum
{
    public int pageIndex;
    public List<MonitorAttributes> attributes;
}

public class ParameterManager : MonoBehaviour
{
    [Header("Default Attributes")]
    [TextArea] public string json;
    public List<MonitorAttributes> attributes;
    public List<ParameterHandler> parameterHandlers;

    [Header("Monitor Planning Attributes")]
    public bool isParameterPlanning;
    public int maxAttributesPerPage = 4;
    public PlanningManager planningManager;
    public TextMeshProUGUI timelineText;
    public List<string> parameterSlugs;

    public void Setup(string json)
    {
        this.json = json;
        if (parameterHandlers.Count == 0)
        {
            parameterHandlers = GetComponentsInChildren<ParameterHandler>().ToList();
        }

        if (isParameterPlanning) SetupAllParameterPlanning();
        else SetupParameterBySlug();
    }

    void SetupAllParameterPlanning()
    {
        var jsonNode = JSON.Parse(json);
        var startDate = DateTime.Parse(jsonNode["_meta"]["date"]["start"]);
        var endDate = DateTime.Parse(jsonNode["_meta"]["date"]["end"]);
        timelineText.text = $"({startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy})";

        var allAttributes = GetAllAttributes(json);
        planningManager.SetupMonitoringPlanningData(CreateMonitorPlanning(allAttributes, maxAttributesPerPage));
        planningManager.SetupPageIndex(0);
    }

    void SetupParameterBySlug()
    {
        attributes.Clear();
        foreach (var item in parameterHandlers)
        {
            if (string.IsNullOrEmpty(item.parameterSlug)) continue;
            attributes.Add(FindAttributeBySlug(json, item.parameterSlug));
        }

        foreach (var item in parameterHandlers)
        {
            var att = attributes.Find(datum => datum.slug == item.parameterSlug);
            if (att == null) continue;

            item.SetParameterCard(att);
        }
    }

    public MonitorAttributes FindAttributeBySlug(string jsonResponse, string slug)
    {
        var response = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(jsonResponse);

        if (response["data"] != null)
        {
            if (response["data"] is Newtonsoft.Json.Linq.JArray dataArray)
            {
                foreach (var item in dataArray)
                {
                    var attributes = item["attributes"];
                    if (attributes != null && attributes["slug"]?.ToString() == slug)
                    {
                        return JsonConvert.DeserializeObject<MonitorAttributes>(attributes.ToString());
                    }
                }
            }

            if (response["data"] is Newtonsoft.Json.Linq.JObject dataObject)
            {
                var attributes = dataObject["attributes"];
                if (attributes != null)
                {
                    var parameters = attributes["parameters"] as Newtonsoft.Json.Linq.JArray;
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            var paramAttributes = param["attributes"];
                            if (paramAttributes != null && paramAttributes["slug"]?.ToString() == slug)
                            {
                                return JsonConvert.DeserializeObject<MonitorAttributes>(paramAttributes.ToString());
                            }
                        }
                    }
                }
            }
        }
        return null;
    }

    public List<MonitorAttributes> GetAllAttributes(string jsonResponse)
    {
        var attributesList = new List<MonitorAttributes>();
        var response = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(jsonResponse);

        if (response["data"] != null)
        {
            if (response["data"] is Newtonsoft.Json.Linq.JArray dataArray)
            {
                foreach (var item in dataArray)
                {
                    var attributes = item["attributes"];
                    if (attributes != null)
                    {
                        var attributeObject = JsonConvert.DeserializeObject<MonitorAttributes>(attributes.ToString());
                        attributesList.Add(attributeObject);
                    }
                }
            }

            if (response["data"] is Newtonsoft.Json.Linq.JObject dataObject)
            {
                var attributes = dataObject["attributes"];
                if (attributes != null)
                {
                    if (attributes["slug"] != null)
                    {
                        var mainAttribute = JsonConvert.DeserializeObject<MonitorAttributes>(attributes.ToString());
                        attributesList.Add(mainAttribute);
                    }

                    var parameters = attributes["parameters"] as Newtonsoft.Json.Linq.JArray;
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            var paramAttributes = param["attributes"];
                            if (paramAttributes != null)
                            {
                                var parameterObject = JsonConvert.DeserializeObject<MonitorAttributes>(paramAttributes.ToString());
                                attributesList.Add(parameterObject);
                            }
                        }
                    }
                }
            }
        }

        return attributesList;
    }

    public List<MonitorPlanning> CreateMonitorPlanning(List<MonitorAttributes> attributes, int itemsPerPage)
    {
        var groupedAttributes = attributes
            .Where(attr => parameterSlugs.Contains(attr.slug))
            .GroupBy(attr => attr.slug)
            .Select(group => new MonitorPlanning
            {
                slug = group.Key,
                data = SplitIntoPages(
                    group.OrderBy(attr => DateTime.TryParse(attr.date, out var parsedDate) ? parsedDate : DateTime.MinValue).ToList(),
                    itemsPerPage
                )
            })
            .ToList();

        return groupedAttributes;
    }

    private List<MonitorPlanningDatum> SplitIntoPages(List<MonitorAttributes> attributes, int itemsPerPage)
    {
        var pagedData = new List<MonitorPlanningDatum>();
        int totalItems = attributes.Count;

        for (int i = 0; i < totalItems || i % itemsPerPage != 0; i += itemsPerPage)
        {
            // Ambil data untuk halaman ini
            var pageAttributes = attributes.Skip(i).Take(itemsPerPage).ToList();

            // Tambahkan data kosong jika jumlah atribut kurang dari itemsPerPage
            while (pageAttributes.Count < itemsPerPage)
            {
                // Tambahkan placeholder dengan nilai default
                pageAttributes.Add(new MonitorAttributes
                {
                    value = new MonitorValue
                    {
                        actual_production = "",
                        target_production = "",
                        value = "",
                        last_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") // Default waktu sekarang
                    },
                    date = "", // Default tanggal sekarang
                    slug = "",
                    name = "",
                    uom = "",
                    description = "",
                    sourceType = ""
                });
            }

            pagedData.Add(new MonitorPlanningDatum
            {
                pageIndex = pagedData.Count, // Indeks halaman dimulai dari 0
                attributes = pageAttributes
            });
        }

        return pagedData;
    }

}
