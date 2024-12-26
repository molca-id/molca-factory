using UnityEngine;
using Molca;
using System;

namespace InteractiveViewer
{
    public class GameManager : RuntimeSubsystem
    {
        public static GameManager main;

        [SerializeField]
        private Material previewMaterial;
        [SerializeField]
        private Canvas rootCanvas;
        [SerializeField]
        private GameObject statusBar;
        [SerializeField]
        private PKT_ModuleGroupInfo[] moduleGroups;

        private PKT_ModuleInfo _activeModule;
        private PKT_ModuleModel _activeModel;

        public static bool IsModuleValid() => main && ActiveModule != null && ActiveModule.IsDataReady;
        public static bool IsModelValid() => main && ActiveModel != null && ActiveModel.IsReady;
        public static Material PreviewMaterial => main.previewMaterial;
        public static PKT_ModuleGroupInfo[] AllGroups => main.moduleGroups;

        public static PKT_ModuleInfo ActiveModule
        {
            get => main._activeModule;
            set
            {
                Debug.Log($"Setting active module to: {(value == null ? "null" : value.Title)}");
                if (value != null)
                    value.Initialize();
                main._activeModule = value;
            }
        }

        public static PKT_ModuleModel ActiveModel
        {
            get => main._activeModel;
            // Only call set after ActiveModule has been assigned.
            set
            {
                main._activeModel = value;
                if (value != null)
                    RuntimeManager.RunCoroutine(value.Initialize());
            }
        }

        public override void Initialize(Action<IRuntimeSubsystem> finishCallback)
        {
            main = this;

            rootCanvas.scaleFactor = GlobalSettings.UIScale;
            GlobalSettings.main.onUiScaleChanged += SetUIScale;

            for (int i = 0; i < moduleGroups.Length; i++)
            {
                for (int j = 0; j < moduleGroups[i].modules.Length; j++)
                {
                    moduleGroups[i].modules[j].OnReset();
                }
                moduleGroups[i].Initialize();
            }

            Activate();
            finishCallback(this);
        }

        public void ToggleStatusBar(bool value)
        {
            statusBar.SetActive(value);
        }

        private void SetUIScale(float value)
        {
            rootCanvas.scaleFactor = value;
        }
    }
}