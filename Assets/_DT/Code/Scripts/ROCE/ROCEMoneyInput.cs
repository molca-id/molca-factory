using UnityEngine;
using System.Globalization;  // For CultureInfo
using TMPro;  // For TMP_InputField

public class ROCEMoneyInput : MonoBehaviour
{
    public TMP_InputField inputField;
    public string amountText;
    private bool isEditing = false;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(FormatCurrency);
    }

    private void FormatCurrency(string input)
    {
        if (string.IsNullOrEmpty(input) || isEditing) return; // Prevent empty input errors and avoid recursive calls

        isEditing = true;

        // Remove non-numeric characters
        string numericString = input.Replace(".", "")
                                    .Replace(",", "")
                                    .Replace("IDR", "")
                                    .Trim();

        if (long.TryParse(numericString, out long number))
        {
            inputField.text = number.ToString("N0", new CultureInfo("id-ID"));
            amountText = inputField.text; // Store formatted value
            
            // Move caret to the end after slight delay
            Invoke("MoveCaretToEnd", 0.01f);
        }
        else
        {
            inputField.text = "0"; // Reset if invalid input
            Invoke("MoveCaretToEnd", 0.01f);
        }
        
        isEditing = false;
    }

    private void MoveCaretToEnd()
    {
        inputField.caretPosition = inputField.text.Length;
    }
}
