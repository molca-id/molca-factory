using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using NaughtyAttributes;
using Molca;

namespace InteractiveViewer
{
    [CreateAssetMenu(fileName = "New ModuleInfo", menuName = "PKT/Module Info")]
    public class PKT_ModuleInfo : ScriptableObject
    {
        public string id;
        public string type;
        public PKT_DynamicLocalization localizedTitle;
        public PKT_DynamicLocalization localizedDetail;
        public PKT_MediaInfo[] medias;
        public AssetReference model;
        [Header("Data")]
        //public PKT_ModuleInformation information;
        public PKT_ModuleStructure[] structures;
        public PKT_ModuleMaintenance[] maintenances;
        public PKT_ToolInfo[] tools;
        public List<PKT_MediaInfo> jsas;
        public List<PKT_ModuleDocumentation> documentations;

        private bool _isInitialized;

        public string Title => localizedTitle.String;
        public string Detail => localizedDetail.String;
        public bool IsDataReady { get; private set; }
        public bool IsValidID(string id) => id.Any(e => char.IsDigit(e));

        /// <summary>
        /// Wait for runtime manager initialization before calling this
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            /*detailKey = $"dt_mc-{id}";
            information.detailKey = $"dt_mc_inf-{id}";*/

            Debug.Log($"Loading module: {id}");
            RuntimeManager.RunCoroutine(LoadData());
        }

        internal void OnReset()
        {
            _isInitialized = false;
            IsDataReady = false;
        }

        private IEnumerator LoadData()
        {
            Debug.Log($"{id} loading. waiting for subsystems.");
            yield return new WaitUntil(RuntimeManager.IsReady);

            foreach(var structure in structures)
            {
                foreach (var part in structure.parts)
                {
                    part.localizedTitle.Init($"dt_mc-{id}_str-{structure.id}_pt-{part.id}_title");
                    part.localizedDetail.Init($"dt_mc-{id}_str-{structure.id}_pt-{part.id}_detail");
                }
            }

            foreach(var maintenance in maintenances)
            {
                foreach (var step in maintenance.steps)
                {
                    step.localizedTitle.Init($"dt_mc-{id}_mt-{maintenance.id}_t-{maintenance.type}_stp-{step.id}_title");
                    step.localizedDetail.Init($"dt_mc-{id}_mt-{maintenance.id}_t-{maintenance.type}_stp-{step.id}_detail");
                }
            }
            IsDataReady = true;
        }

        [Button]
        public void ValidateID()
        {
            foreach (var mtn in maintenances)
            {
                char idPrefix = '-';
                switch (mtn.type)
                {
                    case PKT_ModuleMaintenance.MaintenanceType.None:
                        break;
                    case PKT_ModuleMaintenance.MaintenanceType.Disassembly:
                        idPrefix = 'D';
                        break;
                    case PKT_ModuleMaintenance.MaintenanceType.Assembly:
                        idPrefix = 'A';
                        break;
                    case PKT_ModuleMaintenance.MaintenanceType.Repair:
                        break;
                    case PKT_ModuleMaintenance.MaintenanceType.Overhaul:
                        break;
                }
                if (idPrefix == '-')
                    continue;

                if (mtn.id.Length > 0 && mtn.id[0] != idPrefix)
                    mtn.id = mtn.id.Insert(0, $"{idPrefix}");
                foreach (var stp in mtn.steps)
                {
                    if (stp.id.Length > 0 && stp.id[0] != idPrefix)
                        stp.id = stp.id.Insert(0, $"{idPrefix}");
                }
            }
        }
    }

    [Serializable]
    public class PKT_ModuleStructure
    {
        public string id;
        public string componentKey;
        public PKT_DynamicLocalization localizedTitle;
        public PKT_DynamicLocalization localizedDetail;
        public PKT_MediaInfo[] medias;
        public PKT_ModulePart[] parts;

        public string Title => localizedTitle.String;
        public string Detail => localizedDetail.String;
    }

    [Serializable]
    public class PKT_ModulePart
    {
        public string id;
        public string componentKey;
        public PKT_DynamicLocalization localizedTitle;
        public PKT_DynamicLocalization localizedDetail;
        public PKT_MediaInfo[] medias;

        public string Title => localizedTitle.String;
        public string Detail => localizedDetail.String;
    }

    [Serializable]
    public class PKT_ModuleMaintenance
    {
        public enum MaintenanceType
        {
            None = 0,
            Disassembly = 1, 
            Assembly = 2, 
            Repair = 3, 
            Overhaul = 4
        }

        public string id;
        public MaintenanceType type;
        public PKT_DynamicLocalization localizedTitle;
        public PKT_ModuleMaintenanceStep[] steps;

        public string Title => (LocalizationManager.Language == LocalizationManager.ENGLISH) ? localizedTitle.en : localizedTitle.id;
    }

    [Serializable]
    public class PKT_ModuleMaintenanceStep
    {
        public string id;

        public PKT_DynamicLocalization localizedTitle;
        public PKT_DynamicLocalization localizedDetail;

        public PKT_MediaInfo[] medias;

        public Action<bool> onStateChanged;
        private bool _state;

        public string Title => localizedTitle.String;
        public string Detail => localizedDetail.String;

        public bool IsDone
        {
            get => _state;
            set
            {
                _state = value;
                onStateChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Check if active model has animation with Step's ID as key
        /// </summary>
        public bool HasAnimation => GameManager.ActiveModel.AnimHelper.HasState(id);
    }

    [Serializable]
    public class PKT_ModuleDocumentation
    {
        public string title;
        public int id;
        public List<PKT_MediaInfo> subDocs;

        public PKT_ModuleDocumentation(int id, string title)
        {
            this.title = title;
            this.id = id;
            subDocs = new List<PKT_MediaInfo>();
        }
    }
}