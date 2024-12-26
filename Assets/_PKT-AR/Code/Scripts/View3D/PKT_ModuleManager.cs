using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using Molca;
using Molca.Modals;
using UnityEngine.UI;
using System.Collections;

namespace InteractiveViewer
{
    public class PKT_ModuleManager : MonoBehaviour
    {
        [Header("General")]
        [SerializeField]
        private GameObject uiRoot;
        [SerializeField]
        private GameObject floor;
        [SerializeField]
        private Transform modelRoot;
        [SerializeField]
        private CameraController camController;
        [SerializeField]
        private PKT_ConfirmationHelper modelFailConfirmation;

        [Header("User Interface")]
        [SerializeField]
        private Canvas mainCanvas;
        [SerializeField]
        private PKT_ButtonState[] qualityButtonStates;
        [SerializeField]
        private PKT_ButtonState[] uiScaleButtonStates;
        [SerializeField]
        private HierarchyItem[] hierarchies;
        [SerializeField]
        private Button toggleEnvironmentButton;
        [SerializeField]
        private EventSystem eventSystem;

        [Header("Content Handler")]
        [SerializeField]
        private UIReference informationRef;
        private ModalManager _modal;
        private PKT_ModuleInfo _module;

        private static bool ViewEnvironment = true;

        private IEnumerator Start()
        {
            Debug.Log("Preparing module");
            //Screen.orientation = ScreenOrientation.LandscapeLeft;
            for (int i = 0; i < hierarchies.Length; i++)
            {
                hierarchies[i].Initialize();
            }

            yield return Initialize();

            // Activate UI after inialization completed
            uiRoot.SetActive(true);
            _modal.HideFullScreenLoading();
            Debug.Log("Module is ready.");
        }

        private IEnumerator Initialize()
        {
            yield return new WaitUntil(RuntimeManager.IsReady);

            GlobalSettings.main.onQualityChanged += OnQualityChanged; // Show full screen loading during expensive changes
            //NetworkManager.onConnectionError += OnConnectionError;

            _modal = RuntimeManager.GetSubsystem<ModalManager>();

            string fLoadingMessage = LocalizationManager.Language == LocalizationManager.ENGLISH ? "Preparing Module Data" : "Menyiapkan Data Modul";
            _modal.ShowFullScreenLoading(fLoadingMessage);

            GameManager.main.ToggleStatusBar(false);

            for (int i = 0; i < qualityButtonStates.Length; i++)
            {
                int index = i;
                qualityButtonStates[index].onStateOn.AddListener(() => GlobalSettings.SetQuality(index));
                if (index == GlobalSettings.Quality)
                    qualityButtonStates[index].SetState(true);
            }

            mainCanvas.scaleFactor = GlobalSettings.UIScale;
            switch (GlobalSettings.UIScaleNormalized)
            {
                case 0f:
                    uiScaleButtonStates[0].isOn = true;
                    break;
                case .5f:
                    uiScaleButtonStates[1].isOn = true;
                    break;
                case 1f:
                    uiScaleButtonStates[2].isOn = true;
                    break;
                default:
                    uiScaleButtonStates[0].isOn = true;
                    break;
            }

            while (GameManager.ActiveModule == null)
                yield return new WaitForSeconds(.1f);
            _module = GameManager.ActiveModule;
            StartCoroutine(InstantiateModel());

            #region Module Data Preparation
            while (!GameManager.IsModuleValid())
            {
                _modal.ShowFullScreenLoading(fLoadingMessage);
                yield return new WaitForSeconds(.1f);
            }
            #endregion

            informationRef.GetComponent<HierarchyItem>().title = _module.localizedTitle.String;
            informationRef.GetText("detail").SetText(_module.localizedDetail.String);
            informationRef.GetObject("hierarchy").GetComponent<HierarchyItem>().onActivate.AddListener(() =>
            {
                informationRef.GetObject("previewList").GetComponent<MediaPreviewListUI>().LoadPreviews(_module.medias);
            });

            #region Model Preparation
            _modal.ShowFullScreenLoading("Preparing Model Data");
            yield return new WaitUntil(GameManager.IsModelValid);

            if(GameManager.ActiveModel.environment != null)
            {
                var enviStateButton = toggleEnvironmentButton.GetComponent<PKT_ButtonState>();
                enviStateButton.onStateChanged.AddListener(ToggleEnvironment);
                enviStateButton.isOn = ViewEnvironment;
            }
            else
            {
                toggleEnvironmentButton.gameObject.SetActive(false);
            }
            #endregion
        }

        private void OnDisable()
        {
            GlobalSettings.main.onQualityChanged -= OnQualityChanged;
            //NetworkManager.onConnectionError -= OnConnectionError;
        }

        private void OnConnectionError(string error)
        {
            _modal.AddConfirmation("Connection Error", "Please check your internet connection.", yesText: "Okay");
        }

        private void OnQualityChanged(int value)
        {
            IEnumerator coroutineInternal()
            {
                _modal.ShowFullScreenLoading("Please wait");
                yield return new WaitForSeconds(.4f);
                _modal.HideFullScreenLoading();
            }
            StartCoroutine(coroutineInternal());
        }

        private IEnumerator InstantiateModel()
        {
            string loadingKey = $"Loading model: {_module.model.AssetGUID}";
            var loading = _modal.AddLoading(loadingKey);

            var loadHandle = _module.model.InstantiateAsync(modelRoot);
            while(!loadHandle.IsDone)
            {
                yield return new WaitForEndOfFrame();
                loading.Refresh($"{loadingKey} ({loadHandle.PercentComplete:P})", loadHandle.PercentComplete);
            }

            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                GameManager.ActiveModel = loadHandle.Result.GetComponent<PKT_ModuleModel>();
                yield return new WaitForEndOfFrame();
                ResetViewTransform();
                Debug.Log($"Model loaded: {_module.model.AssetGUID}");
            }
            else
            {
                modelFailConfirmation.Create();
            }
            yield return new WaitForSeconds(.2f);
            _modal.RemoveLoading(loadingKey);
        }

        private void OnDestroy()
        {
            if(GameManager.ActiveModel != null)
                _module.model.ReleaseInstance(GameManager.ActiveModel.gameObject);

            GameManager.ActiveModel = null;
        }

        #region 3D VIEWER
        public void SetModelPreviewMode(bool value)
        {
            if(GameManager.ActiveModel != null)
                GameManager.ActiveModel.SetPreviewMode(value);
        }

        public void SetUIScale(float value)
        {
            GlobalSettings.UIScaleNormalized = value;
            mainCanvas.scaleFactor = GlobalSettings.UIScale;
        }

        public void ResetViewTransform()
        {
            GameManager.ActiveModel.transform.SetParent(modelRoot);
            GameManager.ActiveModel.ResetLocalTransform();
            ResetViewPoint();
            RefreshEnvironment();
        }

        public void ResetViewPoint()
        {
            GameManager.ActiveModel.ResetViewPoint();
        }

        public void ToggleEnvironment(bool value)
        {
            ViewEnvironment = value;
            RefreshEnvironment();
        }

        /// <summary>
        /// Handle environment/floor activation
        /// </summary>
        private void RefreshEnvironment()
        {
            if (!GameManager.IsModelValid()) return;

            bool hasEnvironment = GameManager.ActiveModel.environment != null;

            floor.SetActive(!ViewEnvironment || !hasEnvironment);
            if(hasEnvironment)
                GameManager.ActiveModel.environment.SetActive(ViewEnvironment);
        }
        #endregion
    }
}