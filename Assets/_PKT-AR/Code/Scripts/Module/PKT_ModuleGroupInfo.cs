using Molca;
using UnityEngine;

namespace InteractiveViewer
{
    [CreateAssetMenu(fileName = "New ModuleGroup", menuName = "PKT/Module Group")]
    public class PKT_ModuleGroupInfo : ScriptableObject
    {
        public string id;
        public string brand;
        public string installDate;
        public string type;
        public string area;

        public PKT_DynamicLocalization localizedTitle;
        public PKT_DynamicLocalization localizedDetail;
        
        public PKT_MediaInfo thumbnail;
        public PKT_ModuleInfo[] modules;

        public string Title => localizedTitle.String;
        public string Detail => localizedDetail.String;

        public void Initialize()
        {
            var localManager = RuntimeManager.GetSubsystem<PKT_LocalizationManager>();

            #region Module Group Localization
            localizedTitle.Init($"mcg-{id}_title");
            localizedDetail.Init($"mcg-{id}_detail");
            #endregion

            for (int i = 0; i < modules.Length; i++)
            {
                var mdl = modules[i];
                #region Module Localization
                mdl.localizedTitle.Init($"mcg-{id}_mdl-{mdl.id}_title");
                mdl.localizedDetail.Init($"mcg-{id}_mdl-{mdl.id}_detail");
                #endregion

                #region Module Structure Localization
                foreach (var str in mdl.structures)
                {
                    str.localizedTitle.Init($"mcg-{id}_mdl-{mdl.id}_str-{str.id}_title");
                    str.localizedDetail.Init($"mcg-{id}_mdl-{mdl.id}_str-{str.id}_detail");
                }
                #endregion

                #region Module Maintenance Localization
                foreach (var mtn in mdl.maintenances)
                {
                    mtn.localizedTitle.Init($"mcg-{id}_mdl-{mdl.id}_mtn-{mtn.id}_title");
                }
                #endregion
            }
        }
    }
}