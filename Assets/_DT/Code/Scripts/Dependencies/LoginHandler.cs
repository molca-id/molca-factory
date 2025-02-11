using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginHandler : MonoBehaviour
{
    [Header("Sign In System")]
    public TMP_InputField nameInputField;
    public TMP_InputField passInputField;
    public TextMeshProUGUI errorLog;
    public TextMeshProUGUI descriptionLog;
    public Button signInButton, guestButton;
    public GameObject loginPanel;
    public GameObject sukabumiBlockButton, kejayanBlockButton;

    [Header("Event System")]
    public UnityEvent whenSignedIn;
    public UnityEvent whenOpenFactory;

    [Header("Dummy Datum")]
    [TextArea] public string dummyLoginRes;

    void Start()
    {
        whenSignedIn.AddListener(() => SetupPlantLocation());

        if (!StaticData.need_login && 
            !string.IsNullOrEmpty(PlayerPrefs.GetString("UserData")))
        {
            SetSignInState(false);
            StaticData.current_user_data = APIManager.instance.currentUser = 
                JsonUtility.FromJson<UserDatum>(PlayerPrefs.GetString("UserData"));

            string roles = string.Empty;
            foreach (string role in StaticData.current_user_data.role_id)
            {
                roles += $"{role} ";
            }

            StartCoroutine(
                SetupNotifText(
                $"Login successfully as {roles}",
                true
                ));

            APIManager.instance.QueueRequest(
                APIManager.instance.RefreshToken(whenSignedIn)
                );
        }
    }

    public void SetSignInState(bool state) => 
        SetInteractable(
            state, 
            nameInputField, 
            passInputField, 
            signInButton, 
            guestButton
            );

    void SetInteractable(
        bool state, 
        params Selectable[] elements)
    {
        foreach (var element in elements) 
            element.interactable = state;
    }

    public void SetPassState(Transform button)
    {
        passInputField.contentType =
            passInputField.contentType == TMP_InputField.ContentType.Password
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        int index = passInputField.contentType == TMP_InputField.ContentType.Password
            ? 0 : 1;

        button.GetChild(0).gameObject.SetActive(false);
        button.GetChild(1).gameObject.SetActive(false);
        button.GetChild(index).gameObject.SetActive(true);

        passInputField.ForceLabelUpdate();
    }

    public void SignIn()
    {
        SetSignInState(false);
        StartCoroutine(
               SetupNotifText(
               "Please wait, logging in with current user data...",
               true
               ));
    
        string jsonString = $"{{\"username\":\"{nameInputField.text}\"," +
            $"\"password\":\"{passInputField.text}\"}}";
        StartCoroutine(APIManager.instance.PostDataCoroutine(
            "auth/login", jsonString, HandleLoginResponse
            ));
    }

    public void SignOut()
    {
        StaticData.need_login = false;
        StaticData.branchDetail = string.Empty;

        PlayerPrefs.DeleteKey("UserData");
        SceneManager.LoadScene("Interface");
    }

    private void HandleLoginResponse(string res)
    {
        if (!string.IsNullOrEmpty(dummyLoginRes))
            res = dummyLoginRes;

        SetSignInState(true);
        var jsonNode = JSON.Parse(res);
        if (jsonNode["data"] != null)
        {
            StaticData.current_user_data.username = jsonNode["data"]["id"];
            StaticData.current_user_data.fullname = jsonNode["data"]["attributes"]["name"];
            StaticData.current_user_data.position = jsonNode["data"]["attributes"]["postition"];
            StaticData.current_user_data.plant = jsonNode["data"]["attributes"]["plant"];
            StaticData.current_user_data.access_token = jsonNode["data"]["attributes"]["token"]["attributes"]["access"];
            StaticData.current_user_data.refresh_token = jsonNode["data"]["attributes"]["token"]["attributes"]["refresh"];

            string roles = string.Empty;
            StaticData.current_user_data.role_id = new List<string>();
            foreach (JSONNode role in jsonNode["data"]["attributes"]["roles"])
            {
                string roleName = role.Value.ToLower();
                string roleCapitalized = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(roleName);

                StaticData.current_user_data.role_id.Add(roleCapitalized);
                roles += $"{roleCapitalized} ";

                if (roleName == "admin")
                {
                    StaticData.current_user_data.role_id.Add("Manager");
                    roles += $"Manager ";
                }
            }

            APIManager.instance.currentUser = StaticData.current_user_data;
            PlayerPrefs.SetString("UserData", JsonUtility.ToJson(StaticData.current_user_data));
            PlayerPrefs.Save();

            StartCoroutine(
                SetupNotifText(
                $"Login successfully as {roles}",
                true
                ));

            whenSignedIn.Invoke();
        }
        else
        {
            string errorMessage = $"{jsonNode["statusCode"]}: {jsonNode["message"]}";
            StartCoroutine(SetupNotifText(errorMessage, false));
        }
    }

    public void ResetPassword()
    {
        SetSignInState(false);
        loginPanel.SetActive(false);

        string jsonString = $"{{\"username\":\"{nameInputField.text}\"}}";
        StartCoroutine(APIManager.instance.PostDataCoroutine(
            "api/request-reset-password", jsonString, res =>
        {
            SetSignInState(true);
        }));
    }

    void SetupPlantLocation()
    {
        if (!StaticData.current_user_data.role_id.
                Exists(r => r == "Manager"))
        {
            Debug.Log(StaticData.current_user_data.plant.ToLower());
            if (StaticData.current_user_data.plant.ToLower() == "sukabumi")
                kejayanBlockButton.SetActive(true);
            else if (StaticData.current_user_data.plant.ToLower() == "kejayan")
                sukabumiBlockButton.SetActive(true);
        }
    }

    public IEnumerator SetupNotifText(string message, bool state)
    {
        errorLog.text = state ? string.Empty : message;
        descriptionLog.text = state ? message : string.Empty;
        descriptionLog.transform.parent.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        
        descriptionLog.transform.parent.gameObject.SetActive(false);
    }

    public void MoveToScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void MoveToSceneAdditive(string name)
    {
        StartCoroutine(LoadSceneAndInvoke(name));
    }

    private IEnumerator LoadSceneAndInvoke(string sceneName)
    {
        StaticData.branchDetail = sceneName.ToLower();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        whenOpenFactory.Invoke();
    }
}
