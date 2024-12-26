using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Molca
{
    [CreateAssetMenu(fileName = "Global Settings", menuName = "Molca/Global Settings")]
    public class GlobalSettings : ScriptableObject
    {
        public static GlobalSettings main => RuntimeManager.main.GlobalSettings;

        #region USER INTERFACE
        [Header("User Interface")]
        public float minCanvasScale;
        public float maxCanvasScale;
        public Action<float> onUiScaleChanged;

        private static float _uiScale;
        private const string PREF_UI_SCALE = "UI_SCALE";
        #endregion

        #region AUDIO PROPERTIES
        [Header("Audio")]
        public float audioDefaultBGMVolume;
        public float audioDefaultSFXVolume;
        public float audioDefaultVOVolume;
        public AudioMixerGroup audioMixerMaster;
        public AudioMixerGroup audioMixerBGM;
        public AudioMixerGroup audioMixerSFX;
        public AudioMixerGroup audioMixerVO;

        public float AudioMasterVolume { get; private set; }
        public float AudioBGMVolume { get; private set; }
        public float AudioSFXVolume { get; private set; }
        public float AudioVOVolume { get; private set; }
        #endregion

        public Action<int> onQualityChanged;
        private const string PREF_QUALITY = "QUALITY";

        public void Initialize()
        {
            _uiScale = PlayerPrefs.GetFloat(PREF_UI_SCALE, 0f);
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(PREF_QUALITY, 2));

            Debug.Log($"Global Settings loaded: UI_SCALE: {UIScale}, Quality: {Quality}");
        }

        public static int Quality => QualitySettings.GetQualityLevel();
        public static void SetQuality(int value)
        {
            main.onQualityChanged?.Invoke(value);
            QualitySettings.SetQualityLevel(value);
            PlayerPrefs.SetInt(PREF_QUALITY, value);
        }

        public static float UIScale => Mathf.Lerp(main.minCanvasScale, main.maxCanvasScale, _uiScale);

        public static float UIScaleNormalized
        {
            get
            {
                return _uiScale;
            }
            set
            {
                _uiScale = value;
                PlayerPrefs.SetFloat(PREF_UI_SCALE, _uiScale);
                main.onUiScaleChanged?.Invoke(UIScale);
            }
        }
    }
}