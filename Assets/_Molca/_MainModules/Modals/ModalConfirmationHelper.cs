using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Molca.Modals
{
    public class ModalConfirmationHelper : MonoBehaviour
    {
        [SerializeField]
        private string title;
        [SerializeField]
        private string message;
        [SerializeField]
        private string yesText;
        [SerializeField]
        private string noText;
        [SerializeField]
        private UnityEvent confirmCallback;
        [SerializeField]
        private UnityEvent cancelCallback;

        public void Create()
        {
            RuntimeManager.GetSubsystem<ModalManager>().AddConfirmation(title, message, yesText, noText
                , () => confirmCallback?.Invoke()
                , () => cancelCallback?.Invoke());
        }
    }
}