using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Molca.Modals
{
    public class ModalLoading : MonoBehaviour
    {
        public string title { get; private set; }

        [SerializeField]
        private TextMeshProUGUI messageText;
        [SerializeField]
        private Image loadingBar;

        public void Initialize(string title)
        {
            this.title = title;
            messageText.SetText(title);
        }

        public void Refresh(string msg, float progress)
        {
            messageText.SetText(msg);
            loadingBar.fillAmount = progress;
        }
    }
}