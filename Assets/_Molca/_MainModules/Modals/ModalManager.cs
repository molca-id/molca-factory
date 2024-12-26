using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;

namespace Molca.Modals
{
    [RequireComponent(typeof(Canvas))]
    public class ModalManager : RuntimeSubsystem
    {
        private static ModalManager _instance;

        public enum MessageType
        {
            Default = 0,
            Warning = 1,
            Error = 2
        }

        [Header("General")]
        [SerializeField, Tooltip("Subscribe to Logger onLogs event, displaying it as messages.")]
        private bool hookLogger;

        #region MESSAGES PROPERTIES
        [Header("Messages")]
        [SerializeField]
        private RectTransform msgRoot;
        [SerializeField]
        private ModalMessage msgPrefab;
        [SerializeField]
        private Color msgDefaultColor;
        [SerializeField]
        private Color msgWarningColor;
        [SerializeField]
        private Color msgErrorColor;

        private ObjectPool<ModalMessage> _messagePool;
        #endregion

        #region CONFIRMATION PROPERTIES
        [Header("Confirmation")]
        [SerializeField]
        private GameObject cfmPanel;
        [SerializeField]
        private TextMeshProUGUI cfmTitle;
        [SerializeField]
        private TextMeshProUGUI cfmMessage;
        [SerializeField]
        private Button cfmConfirmButton;
        [SerializeField]
        private Button cfmCancelButton;
        [SerializeField]
        private LocalizedString yesLocale;
        [SerializeField]
        private LocalizedString noLocale;

        public static bool IsConfirmationActive => _instance.cfmPanel.activeSelf;
        #endregion

        #region LOADING PROPERTIES
        [Header("Loading")]
        [SerializeField]
        private RectTransform loadingRoot;
        [SerializeField]
        private ModalLoading loadingPrefab;
        [SerializeField]
        private GameObject fullScreenLoadingPanel;
        [SerializeField]
        private TextMeshProUGUI fullScreenLoadingMsg;

        private ObjectPool<ModalLoading> _loadingPool;
        private Dictionary<string, ModalLoading> _activeLoadings;
        #endregion

        private bool validAction(string msg) => isActive && msg.Length > 0;

        public override void Initialize(Action<IRuntimeSubsystem> finishCallback)
        {
            _instance = this;

            if (hookLogger)
            {
                LogManager logger = RuntimeManager.GetSubsystem<LogManager>();
                if (logger != null)
                {
                    logger.onLogInfo += (msg) => { AddMessage(msg, MessageType.Default); };
                    logger.onLogWarning += (msg) => { AddMessage(msg, MessageType.Warning); };
                    logger.onLogError += (msg) => { AddMessage(msg, MessageType.Error); };
                }
            }

            _messagePool = new ObjectPool<ModalMessage>(msgPrefab, 10, msgRoot);
            _loadingPool = new ObjectPool<ModalLoading>(loadingPrefab, 5, loadingRoot);
            _activeLoadings = new Dictionary<string, ModalLoading>();
            cfmPanel.SetActive(false);

            Activate();
            finishCallback?.Invoke(this);

            StartCoroutine(WaitForRuntime());
        }

        IEnumerator WaitForRuntime()
        {
            // Wait for all subsystem
            ShowFullScreenLoading("Preparing system...");
            yield return new WaitUntil(RuntimeManager.IsReady);
            HideFullScreenLoading();
        }

        #region MESSAGES FUNCTION
        public void AddMessage(string message, MessageType msgType = MessageType.Default, float duration = 10f)
        {
            if (!validAction(message))
                return;

            IEnumerator coroutineInternal()
            {
                ModalMessage msg = _messagePool.GetObject();
                msg.transform.SetSiblingIndex(0);
                if (msg == null)
                {
                    yield return _messagePool.IncreaseSizeAsync(2);
                    msg = _messagePool.GetObject();
                }
                msg.Initialize(message, GetMessageColor(msgType));
                yield return ReturnMessage(msg, duration);
            }
            StartCoroutine(coroutineInternal());
        }

        private IEnumerator ReturnMessage(ModalMessage msg, float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            _messagePool.ReturnObject(msg);
        }

        private Color GetMessageColor(MessageType msgType)
        {
            switch (msgType)
            {
                case MessageType.Default:
                    return msgDefaultColor;
                case MessageType.Warning:
                    return msgWarningColor;
                case MessageType.Error:
                    return msgErrorColor;
                default:
                    return msgDefaultColor;
            }
        }
        #endregion

        #region CONFIRMATION FUNCTION
        public void AddConfirmation(string title, string message, string yesText = null, string noText = null, Action confirmCallback = null, Action cancelCallback = null)
        {
            if (!validAction(message))
            {
                Debug.LogError($"Failed to show confirmation: message can't be null.");
                return;
            }

            if (cfmPanel.activeSelf)
            {
                Debug.LogWarning($"Failed to show confirmation for title: {title}. A confirmation modal is already active with title: {cfmTitle.text}");
                return;
            }

            IEnumerator coroutineInternal()
            {
                cfmTitle.gameObject.SetActive(!string.IsNullOrWhiteSpace(title));
                cfmTitle.text = title;
                cfmMessage.text = message;

                string yes = null;
                if(string.IsNullOrEmpty(yesText))
                {
                    var async = yesLocale.GetLocalizedStringAsync();
                    while (!async.IsDone) yield return new WaitForEndOfFrame();
                    yes = async.Result;
                }
                else
                {
                    yes = yesText;
                }

                string no = null;
                if (string.IsNullOrEmpty(noText)) {
                    var async = noLocale.GetLocalizedStringAsync();
                    while (!async.IsDone) yield return new WaitForEndOfFrame();
                    no = async.Result;
                }
                else
                {
                    no = noText;
                }

                cfmConfirmButton.GetComponentInChildren<TextMeshProUGUI>().SetText(yes);
                cfmCancelButton.GetComponentInChildren<TextMeshProUGUI>().SetText(no);

                if (confirmCallback != null)
                    cfmConfirmButton.onClick.AddListener(() => confirmCallback.Invoke());
                if (cancelCallback != null)
                    cfmCancelButton.onClick.AddListener(() => cancelCallback.Invoke());

                cfmCancelButton.gameObject.SetActive(cancelCallback != null || !string.IsNullOrEmpty(noText));
                cfmPanel.SetActive(true);
            }
            StartCoroutine(coroutineInternal());
        }

        public void ClearConfirmation()
        {
            cfmConfirmButton.onClick.RemoveAllListeners();
            cfmCancelButton.onClick.RemoveAllListeners();
            cfmPanel.SetActive(false);
        }
        #endregion

        #region LOADING FUNCTION
        public ModalLoading AddLoading(string title)
        {
            if (!validAction(title))
                return null;

            if (_activeLoadings.ContainsKey(title))
            {
                Debug.LogWarning($"Failed to add loading modal, title '{title}' already exist.");
            }
            else
            {
                ModalLoading loading = _loadingPool.GetObject();
                loading.transform.SetSiblingIndex(0);
                loading.Initialize(title);
                _activeLoadings.Add(title, loading);
            }

            return _activeLoadings[title];
        }

        public void RemoveLoading(string title)
        {
            if (!_activeLoadings.ContainsKey(title))
            {
                Debug.LogWarning($"Failed to remove loading modal, title '{title}' doesn't exist.");
                return;
            }

            _loadingPool.ReturnObject(_activeLoadings[title]);
            _activeLoadings.Remove(title);
        }

        public void ShowFullScreenLoading(string message)
        {
            fullScreenLoadingMsg.SetText(message);
            if(!fullScreenLoadingPanel.activeSelf)
                fullScreenLoadingPanel.SetActive(true);
        }

        public void HideFullScreenLoading()
        {
            fullScreenLoadingPanel.SetActive(false);
        }
        #endregion
    }
}