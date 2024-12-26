using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Molca.Modals
{
    public class ModalMessage : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI messageText;

        private readonly Color _stripeColor = new Color(.8f, .8f, .8f);
        private static int _msgCount;

        public void Initialize(string msg, Color color)
        {
            messageText.text = msg;
            messageText.color = (_msgCount % 2 == 0) ? color : color * _stripeColor;
            _msgCount++;

            LayoutRebuilder.ForceRebuildLayoutImmediate(messageText.rectTransform);
        }
    }
}