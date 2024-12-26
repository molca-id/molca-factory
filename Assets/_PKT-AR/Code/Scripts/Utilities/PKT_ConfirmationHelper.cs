using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Molca;
using Molca.Modals;

namespace InteractiveViewer
{
    public class PKT_ConfirmationHelper : MonoBehaviour
    {
        [SerializeField]
        private PKT_DynamicLocalization title;
        [SerializeField]
        private PKT_DynamicLocalization message;
        [SerializeField]
        private PKT_DynamicLocalization yesText;
        [SerializeField]
        private PKT_DynamicLocalization noText;
        [SerializeField]
        private UnityEvent confirmCallback;
        [SerializeField]
        private UnityEvent cancelCallback;

        private const string LOCALE_KEY_PREFIX = "_Confirmation";

        private IEnumerator Start()
        {
            yield return new WaitUntil(RuntimeManager.IsReady);

            title.Init($"{LOCALE_KEY_PREFIX}.{RandomStringGenerator.GenerateGuid()}");
            message.Init($"{LOCALE_KEY_PREFIX}.{RandomStringGenerator.GenerateGuid()}");
            yesText.Init($"{LOCALE_KEY_PREFIX}.{RandomStringGenerator.GenerateGuid()}");
            noText.Init($"{LOCALE_KEY_PREFIX}.{RandomStringGenerator.GenerateGuid()}");
        }

        public void Create()
        {
            RuntimeManager.GetSubsystem<ModalManager>().AddConfirmation(
                title.String,
                message.String,
                yesText.String,
                noText.String,
                confirmCallback.GetPersistentEventCount() > 0 ? () => confirmCallback?.Invoke() : null,
                cancelCallback.GetPersistentEventCount() > 0 ? () => cancelCallback?.Invoke() : null);
        }

        public void Create(string msg = null)
        {
            RuntimeManager.GetSubsystem<ModalManager>().AddConfirmation(
                title.String,
                string.IsNullOrEmpty(msg) ? message.String : msg,
                yesText.String,
                noText.String,
                confirmCallback.GetPersistentEventCount() > 0 ? () => confirmCallback?.Invoke() : null,
                cancelCallback.GetPersistentEventCount() > 0 ? () => cancelCallback?.Invoke() : null);
        }
    }
}