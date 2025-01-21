using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TNVirtualKeyboard : MonoBehaviour
{
	
	public static TNVirtualKeyboard instance;
	
	public string unameWords = "";
	public string passWords = "";
	
	public GameObject vkCanvas;
	
	public TMP_InputField usernameText;
	public TMP_InputField passwordText;
	public TMP_InputField showText;

    public bool unameIsSelected = false;
    public bool passIsSelected = false;
	
	
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
		HideVirtualKeyboard();

        usernameText.onSelect.AddListener(OnUsernameSelected);
        passwordText.onSelect.AddListener(OnPasswordSelected);
    }

    // Update is called once per frame
    void Update()
    {
        if (unameIsSelected == true & passIsSelected == false)
        {
            showText.contentType = TMP_InputField.ContentType.Standard;
            showText.text = usernameText.text;
        }
        else if (unameIsSelected == false & passIsSelected == true)
        {
            showText.contentType = TMP_InputField.ContentType.Password;
            showText.text = passwordText.text;
        }
    }
	
	public void KeyPress(string k){
        if (unameIsSelected == true & passIsSelected == false)
		{
			showText.text = "";
            showText.contentType = TMP_InputField.ContentType.Standard;
            unameWords += k;
            usernameText.text = unameWords;
            showText.text = usernameText.text;
        }
		else if (unameIsSelected == false & passIsSelected == true)
		{
            showText.text = "";
			showText.contentType = TMP_InputField.ContentType.Password;
            passWords += k;
            passwordText.text = passWords;
            showText.text = passwordText.text;
        }
			
	}
	
	public void Del(){


        if (unameIsSelected == true & passIsSelected == false)
        {
            unameWords = unameWords.Remove(unameWords.Length - 1, 1);
            usernameText.text = unameWords;
            showText.text = usernameText.text;
        }
        else if (unameIsSelected == false & passIsSelected == true)
        {
            passWords = passWords.Remove(passWords.Length - 1, 1);
            passwordText.text = passWords;
            showText.text = passwordText.text;
        }
        
	}
	
	public void ShowVirtualKeyboard(){
		vkCanvas.SetActive(true);
	}
	
	public void HideVirtualKeyboard(){
		vkCanvas.SetActive(false);
	}

    public void OnUsernameSelected(string text)
    {
        Debug.Log("Username input field selected.");
        unameIsSelected = true;
        passIsSelected = false;
    }

    public void OnPasswordSelected(string text)
    {
        Debug.Log("Password input field selected.");
        unameIsSelected = false;
        passIsSelected = true;
    }
}
