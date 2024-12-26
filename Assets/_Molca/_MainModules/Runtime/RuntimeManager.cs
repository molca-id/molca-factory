using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Molca
{
    public class RuntimeManager : MonoBehaviour
    {
        private static RuntimeManager _main;

        [SerializeField]
        private GlobalSettings _globalSettings;

        private RuntimeSubsystem[] _subsystems;
        private bool _isReady;

        public static RuntimeManager main => _main;
        public static bool IsReady() => main != null && main._isReady;
        public GlobalSettings GlobalSettings => _globalSettings;

        #region Initialize
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void NullCheck()
        {
            if (_main == null)
                GetInstance();
        }

        private static void GetInstance()
        {
            string address = "Assets/_AIO/Level/Prefabs/Runtime Manager.prefab"; // should get address from a file
            Addressables.LoadAssetAsync<GameObject>(address).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _main = Instantiate(handle.Result).GetComponent<RuntimeManager>();
                    RunCoroutine(_main.Initialize());
                    DontDestroyOnLoad(_main.gameObject);
                }
                else
                {
                    Debug.LogError("Failed to load asset: " + address);
                }
            };
        }

        private IEnumerator Initialize()
        {
            Debug.Log("Initializing runtime manager.");
            _main._globalSettings.Initialize();
            Debug.Log("Global setting initialized.");

            bool shouldContinue = false;
            void SubsystemInitializedCallback(IRuntimeSubsystem subsystem) { shouldContinue = true; }

            _subsystems = GetComponentsInChildren<RuntimeSubsystem>();
            for (int i = 0; i < _subsystems.Length; i++)
            {
                shouldContinue = false;
                Debug.Log($"Initialize subsystem of type: {_subsystems[i].GetType()}. ({i}/{_subsystems.Length})");
                _subsystems[i].Initialize(SubsystemInitializedCallback);
                while (!shouldContinue)
                    yield return new WaitForEndOfFrame();
            }
            _isReady = true;
            Debug.Log("Runtime manager initialized.");
        }
        #endregion

        public static T GetSubsystem<T>() where T : RuntimeSubsystem
        {
            for (int i = 0; i < main._subsystems.Length; i++)
            {
                if (main._subsystems[i] is T)
                    return main._subsystems[i] as T;
            }
            return null;
        }

        public static Coroutine RunCoroutine(IEnumerator enumerator)
        {
            return main.StartCoroutine(enumerator);
        }
    }
}