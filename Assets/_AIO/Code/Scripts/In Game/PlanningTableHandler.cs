using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PlanningTableHandler : MonoBehaviour
{
    public TextMeshProUGUI currentPcsText;
    public TextMeshProUGUI targetPcsText;
    public TextMeshProUGUI achievementRateText;

    public void SetupTable(
        string currentPcs = "",
        string targetPcs = ""
    )
    {
        if (string.IsNullOrEmpty(currentPcs) ||
            string.IsNullOrEmpty(targetPcs))
        {
            currentPcsText.text =
                targetPcsText.text =
                achievementRateText.text = "•••";
        }
        else
        {
            int currentPcsValue = Convert.ToInt32(currentPcs);
            int targetPcsValue = Convert.ToInt32(targetPcs);

            //int currentPcsValue = 100;
            //int targetPcsValue = 50;

            currentPcsText.text = currentPcsValue.ToString("N0");
            targetPcsText.text = targetPcsValue.ToString("N0");

            // Logika untuk menampilkan persentase atau "N/A"
            if (targetPcsValue != 0)
            {
                float achievementRate = ((float)currentPcsValue / targetPcsValue) * 100;
                achievementRateText.text = $"{achievementRate.ToString("F1")}%";
            }
            else if (currentPcsValue != 0)
            {
                float achievementRate = currentPcsValue * 100; // Jika targetPcs adalah 0, persentase berdasarkan currentPcs
                achievementRateText.text = $">{achievementRate.ToString("F1")}%";
            }
            else
            {
                achievementRateText.text = "0%"; // Jika keduanya nol
            }
        }
    }
}
