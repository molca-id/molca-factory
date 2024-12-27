using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class APIManager : MonoBehaviour
{
    public static APIManager instance;
    public UserDatum currentUser;

    private static bool isRefreshingToken = false; // Lock untuk refresh token
    public List<IEnumerator> pendingRequests = new List<IEnumerator>(); // Pending requests

    private void Awake()
    {
        instance = this;
    }

    string SetupUri(string subUri)
    {
        if (!string.IsNullOrEmpty(StaticData.branchDetail))
        {
#if UNITY_EDITOR
            //return string.Format("{0}/{1}/{2}", StaticData.aioDomain, StaticData.branchDetail, subUri);
            return string.Format("{0}/{1}/{2}", StaticData.molcaDomain, StaticData.branchDetail, subUri);
#else
            //return string.Format("{0}/{1}/{2}", StaticData.aioDomain, StaticData.branchDetail, subUri);
            return string.Format("{0}/{1}/{2}", StaticData.molcaDomain, StaticData.branchDetail, subUri);
#endif
        }
        else
        {
#if UNITY_EDITOR
            //return string.Format("{0}/{1}", StaticData.aioDomain, subUri);
            return string.Format("{0}/{1}", StaticData.molcaDomain, subUri);
#else
            //return string.Format("{0}/{1}", StaticData.aioDomain, subUri);
            return string.Format("{0}/{1}", StaticData.molcaDomain, subUri);
#endif
        }
    }

    public void QueueRequest(IEnumerator request)
    {
        StartCoroutine(ProcessRequest(request));
    }

    private IEnumerator ProcessRequest(IEnumerator request)
    {
        if (isRefreshingToken)
        {
            if (!pendingRequests.Contains(request))
            {
                pendingRequests.Add(request);
            }
            yield break;
        }

        yield return StartCoroutine(request);
    }

    public IEnumerator PostDataCoroutine(
        string subUri,
        string jsonData,
        Action<string> SetDataEvent = null
        )
    {
        string url = SetupUri(subUri);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
            SetDataEvent?.Invoke(request.downloadHandler.text);
        }
        else
        {
            SetDataEvent?.Invoke(request.downloadHandler.text);
        }
    }

    public IEnumerator GetDataCoroutine(
        string subUri,
        Action<string> SetDataEvent = null
        )
    {
        string url = SetupUri(subUri);
        string header = "Bearer " + StaticData.current_user_data.access_token;

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", header);

        yield return request.SendWebRequest();

        if ((request.responseCode == 401 ||
            request.responseCode == 403) &&
            !isRefreshingToken)
        {
            isRefreshingToken = true;
            yield return StartCoroutine(RefreshToken());
        }
        else if (request.result == UnityWebRequest.Result.Success)
        {
            SetDataEvent?.Invoke(request.downloadHandler.text);
        }
    }

    public IEnumerator RefreshToken(UnityEvent events = null)
    {
        string refreshUrl = SetupUri("auth/access-token");
        string header = "Bearer " + StaticData.current_user_data.refresh_token;

        UnityWebRequest refreshRequest = UnityWebRequest.Get(refreshUrl);
        refreshRequest.SetRequestHeader("Authorization", header);

        yield return refreshRequest.SendWebRequest();

        if (refreshRequest.result == UnityWebRequest.Result.Success)
        {
            StaticData.current_user_data.access_token = ExtractValue(refreshRequest.downloadHandler.text, "access");
            StaticData.current_user_data.refresh_token = ExtractValue(refreshRequest.downloadHandler.text, "refresh");
            PlayerPrefs.SetString("UserData", JsonUtility.ToJson(StaticData.current_user_data));
            PlayerPrefs.Save();

            foreach (var req in pendingRequests)
            {
                StartCoroutine(req);
            }

            Debug.Log($"Successfully Update Tokens!");
            pendingRequests.Clear();
            events?.Invoke();
        }
        else
        {
            StaticData.need_login = true;
            StaticData.branchDetail = string.Empty;
            SceneManager.LoadScene("Interface");

            Debug.Log($"Failed Update Tokens! {refreshRequest.error}");
            pendingRequests.Clear();
        }

        isRefreshingToken = false;
    }

    public IEnumerator DownloadImageCoroutine(string imageUrl, Action<Sprite> SetDataEvent = null)
    {
        string refreshUrl = SetupUri(imageUrl);
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(refreshUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading image: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            SetDataEvent(sprite);
        }
    }

    public static string ExtractValue(string jsonResponse, string key)
    {
        try
        {
            // Parse JSON menggunakan Newtonsoft.Json
            JObject parsedJson = JObject.Parse(jsonResponse);

            // Rekursif mencari properti berdasarkan key
            JToken token = FindToken(parsedJson, key);
            return token?.ToString();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing JSON: {ex.Message}");
            return null;
        }
    }

    private static JToken FindToken(JToken token, string key)
    {
        if (token.Type == JTokenType.Object)
        {
            foreach (var property in token.Children<JProperty>())
            {
                if (property.Name == key)
                {
                    return property.Value;
                }

                // Rekursif ke dalam properti anak
                var foundToken = FindToken(property.Value, key);
                if (foundToken != null)
                {
                    return foundToken;
                }
            }
        }
        else if (token.Type == JTokenType.Array)
        {
            foreach (var item in token.Children())
            {
                var foundToken = FindToken(item, key);
                if (foundToken != null)
                {
                    return foundToken;
                }
            }
        }
        return null;
    }
}
