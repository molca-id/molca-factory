using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Molca;

namespace InteractiveViewer
{
    public class PKT_ModuleMaintenanceUI : MonoBehaviour
    {
        [Header("Hierarchy")]
        [SerializeField]
        private HierarchyItem hierarchyProcedureTypeList;
        [SerializeField]
        private HierarchyItem hierarchyProcedureList;
        [SerializeField]
        private HierarchyItem hierarchyStepCheckList;
        [SerializeField]
        private HierarchyItem hierarchyStepDetail;

        [Header("Procedures")]
        [SerializeField]
        private Transform procedureItemRoot;
        [SerializeField]
        private UIReference procedureItemTemplate;

        [Header("Procedure Steps")]
        [SerializeField]
        private BooleanColor stepStateColor;
        [SerializeField]
        private Transform stepItemRoot;
        [SerializeField]
        private UIReference stepItemTemplate;
        [SerializeField]
        private UIReference stepDetailReference;
        [SerializeField]
        private UIReference popUpUI;
        [SerializeField]
        private MediaPreviewListUI previewListUI;
        [SerializeField]
        private ProgressBarUI animProgressUI;
        [SerializeField]
        private PKT_ButtonState playAllAnimButton;
        [SerializeField]
        private PKT_ButtonState playButton;

        private ObjectPool<UIReference> _procedureItemPool;
        private ObjectPool<UIReference> _stepItemPool;

        private PKT_ModulePathUI _pathUI;
        private CameraController _camController;
        private AnimatorHelper _animHelper;
        private Button _nextStepButton;
        private Button _prevStepButton;
        private PKT_ModuleMaintenance[] _procedures;
        private bool _autoPlay => _animHelper.AutoPlay;
        private int _procedureIndex = -1;
        private int _stepIndex = -1;

        public PKT_ModuleMaintenance.MaintenanceType SelectedType { get; private set; }
        public PKT_ModuleMaintenance SelectedProcedure => _procedureIndex < 0 ? null : _procedures[_procedureIndex];
        public PKT_ModuleMaintenanceStep SelectedStep => _stepIndex < 0 ? null : SelectedProcedure.steps[_stepIndex];

        private void Start()
        {
            _pathUI = FindFirstObjectByType<PKT_ModulePathUI>();
            _camController = FindFirstObjectByType<CameraController>();
            animProgressUI.onProgressFilled += OnAnimationEnd;
            playAllAnimButton.onStateChanged.AddListener((value) =>
            {
                if (value == _autoPlay) return;

                if (value == true)
                {
                    animProgressUI.Title = LocalizationManager.Language == LocalizationManager.ENGLISH ? "Playing all animations" : "Memainkan semua animasi";
                    ResetModelState();
                }

                _animHelper.AutoPlay = value;
                ToggleAnimator(value);
            });

            hierarchyProcedureTypeList.onActivate.AddListener(() =>
            {
                ToggleAnimator(false);
                _animHelper.ResetAll();
                _pathUI.AddPath(hierarchyProcedureTypeList);
            });
            hierarchyProcedureList.onActivate.AddListener(() => _pathUI.AddPath(hierarchyProcedureList));
            hierarchyStepCheckList.onActivate.AddListener(() => _pathUI.AddPath(hierarchyStepCheckList));
            hierarchyStepDetail.onActivate.AddListener(() => _pathUI.AddPath(hierarchyStepDetail));

            StartCoroutine(Initialize());
        }

        /// <summary>
        /// Initialize item pools and button functions
        /// </summary>
        public IEnumerator Initialize()
        {
            yield return new WaitUntil(GameManager.IsModuleValid);
            yield return new WaitUntil(GameManager.IsModelValid);

            _procedureItemPool = new ObjectPool<UIReference>(procedureItemTemplate, 5, procedureItemRoot);
            _stepItemPool = new ObjectPool<UIReference>(stepItemTemplate, 10, stepItemRoot);

            stepDetailReference.GetButton("btn.task").onClick.AddListener(() =>
            {
                SelectedStep.IsDone = !SelectedStep.IsDone;
                stepDetailReference.GetImage("bg.task").color = stepStateColor.GetColor(SelectedStep.IsDone);
            });

            _nextStepButton = stepDetailReference.GetButton("btn.next");
            _prevStepButton = stepDetailReference.GetButton("btn.prev");

            _nextStepButton.onClick.AddListener(() => IncrementStepIndex(1));
            _prevStepButton.onClick.AddListener(() => IncrementStepIndex(-1));

            _animHelper = GameManager.ActiveModel.AnimHelper;
            _animHelper.onPlayStateChanged += playButton.SetState;

            animProgressUI.onValueChanged.AddListener(_animHelper.SetAnimationProgress);
            _animHelper.onAnimProgress += animProgressUI.UpdateProgress;
            ToggleAnimator(false);
        }

        /// <summary>
        /// Select procedure type and load all procedure items of selected type
        /// </summary>
        /// <param name="type"></param>
        public void SelectType(int mType)
        {
            if (SelectedType != (PKT_ModuleMaintenance.MaintenanceType)mType)
            {
                SelectedType = (PKT_ModuleMaintenance.MaintenanceType)mType;

                _procedureItemPool.ReturnAll();
                _procedures = GameManager.ActiveModule.maintenances.Where(mt => mt.type == SelectedType).ToArray();
                for (int i = 0; i < _procedures.Length; i++)
                {
                    InitializeProcedureItem(_procedureItemPool.GetObject(), i);
                    for (int j = 0; j < _procedures[i].steps.Length; j++)
                        _procedures[i].steps[j].IsDone = false;
                }
            }

            _animHelper.FilterStates(SelectedType);

            hierarchyProcedureTypeList.Deactivate(false);
            hierarchyProcedureList.Activate(false);
            hierarchyProcedureList.title = SelectedType.ToString();
            _pathUI.RefreshLabel();
        }

        /// <summary>
        /// Initialize procedure item's title and button function
        /// </summary>
        /// <param name="item"></param>
        /// <param name="procedure"></param>
        /// <param name="index"></param>
        private void InitializeProcedureItem(UIReference item, int index)
        {
            var prd = _procedures[index];
            /*Debug.Log($"Loading procedure item, data =>" +
                $"\r\ntitle: {prd.Title}" +
                $"\r\nid: {prd.id}" +
                $"\r\ntitle dynamic: {prd.localizedTitle.id}");*/
            item.transform.SetSiblingIndex(index);
            item.GetText("title").SetText(_procedures[index].Title);
            item.GetButton("button").onClick.RemoveAllListeners();
            item.GetButton("button").onClick.AddListener(() => SelectProcedure(index));
        }

        /// <summary>
        /// Select procedure and load all step items of selected procedure
        /// </summary>
        /// <param name="procedure"></param>
        public void SelectProcedure(int pIndex)
        {
            if (SelectedProcedure == _procedures[pIndex]) return;
            ResetModelState(); // Reset model state from the previous procedure

            _procedureIndex = pIndex;
            hierarchyStepCheckList.title = SelectedProcedure.Title;
            _pathUI.RefreshLabel();

            _stepIndex = -1;
            _stepItemPool.ReturnAll();
            for (int i = 0; i < SelectedProcedure.steps.Length; i++)
            {
                int index = i;
                PKT_ModuleMaintenanceStep step = SelectedProcedure.steps[index];

                UIReference item = _stepItemPool.GetObject();
                item.transform.SetSiblingIndex(index);
                item.GetText("number").text = $"{index + 1}.";
                item.GetText("title").text = step.Title;
                item.GetImage("bg.check").color = stepStateColor.GetColor(step.IsDone);
                item.GetButton("btn.check").onClick.RemoveAllListeners();
                item.GetButton("btn.check").onClick.AddListener(() => step.IsDone = !step.IsDone);
                item.GetButton("btn.detail").onClick.RemoveAllListeners();
                item.GetButton("btn.detail").onClick.AddListener(() => LoadStepDetail(index));
                item.GetObject("animIndicator").SetActive(step.HasAnimation);
                step.onStateChanged = null;
                step.onStateChanged += (st) => item.GetImage("bg.check").color = stepStateColor.GetColor(st);
            }
        }

        /// <summary>
        /// Select step and load its detail, move camera and play animation if available
        /// </summary>
        /// <param name="index"></param>
        /// <param name="step"></param>
        private void LoadStepDetail(int index)
        {
            var step = SelectedProcedure.steps[index];
            if (SelectedStep == step)
            {
                if(SelectedStep != null)
                    PrepareCurrentStepAnimation();
                return;
            }

            // Adjust current step's animation based on the new index
            if (SelectedStep != null && SelectedStep.HasAnimation)
            {
                var animState = _animHelper.GetStateByName(SelectedStep.id);
                if (index < _stepIndex)
                    animState.GoToStart();
                else
                    animState.GoToEnd();
            }

            _stepIndex = index;
            CheckStepButtons();

            hierarchyStepDetail.title = step.Title;
            _pathUI.RefreshLabel();

            stepDetailReference.GetText("step").text = $"{index + 1} / {SelectedProcedure.steps.Length}";
            stepDetailReference.GetText("title").text = step.Title;
            stepDetailReference.GetText("detail").text = step.Detail;
            LayoutRebuilder.ForceRebuildLayoutImmediate(stepDetailReference.GetText("detail").rectTransform);
            stepDetailReference.GetImage("bg.task").color = stepStateColor.GetColor(step.IsDone);
            // stepDetailReference.GetButton("btn.anim").gameObject.SetActive(SelectedStep.HasAnimation);

            PrepareCurrentStepAnimation();
            previewListUI.LoadPreviews(step.medias);
        }

        private void IncrementStepIndex(int increment)
        {
            LoadStepDetail((_stepIndex + increment) % SelectedProcedure.steps.Length);
        }

        private void CheckStepButtons()
        {
            _prevStepButton.interactable = _stepIndex > 0;
            _nextStepButton.interactable = _stepIndex < SelectedProcedure.steps.Length - 1;
        }

        public void ToggleAnimator(bool value)
        {
            if (!_animHelper)
                return;

            if (!value)
            {
                _animHelper.PauseAnimation();
                playAllAnimButton.isOn = false;
            }
            else
                _animHelper.ResumeAnimation();

            animProgressUI.UpdateProgress(0f);
            animProgressUI.gameObject.SetActive(value);
        }

        public void StopAnimation()
        {
            _animHelper.PauseAnimation();
            _animHelper.SetAnimationProgress(0f);
        }

        public void ToggleLooping(bool value)
        {
            if (!_animHelper) return;
            _animHelper.IsLooping = value;
        }

        public void ToggleModelAnimation(bool value)
        {
            if (!_animHelper) return;

            if (value)
                _animHelper.ResumeAnimation();
            else
                _animHelper.PauseAnimation();
        }

        public void PrepareCurrentStepAnimation()
        {
            animProgressUI.Title = SelectedStep.Title;
            _animHelper.AutoPlay = false;
            _animHelper.SetState(SelectedStep.HasAnimation ? SelectedStep.id : null);
            ToggleAnimator(SelectedStep.HasAnimation);
        }

        public void ResetModelState()
        {
            if(SelectedProcedure == null) return;

            ToggleModelAnimation(false);
            for(int i = SelectedProcedure.steps.Length - 1; i >= 0; i--)
            {
                var step = SelectedProcedure.steps[i];
                if (step.HasAnimation)
                    _animHelper.SetStateProgress(step.id, 0f);
            }
        }

        private void OnAnimationEnd()
        {
            if (!Application.isPlaying)
                return;
            _animHelper.PauseAnimation();
        }
    }
}