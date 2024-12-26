using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Molca.Utils
{
    [CreateAssetMenu(fileName = "Scene Utility", menuName = "Molca/Utils/Scene Utility")]
    public class SceneUtility : ScriptableObject
    {
        public void LoadScene(string sceneName)
        {
            SceneLoadManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void LoadScene(SharedString sharedString)
        {
            SceneLoadManager.LoadScene(sharedString.value, LoadSceneMode.Single);
        }

        public void LoadSceneAdditive(string sceneName)
        {
            SceneLoadManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public void LoadSceneAdditive(SharedString sharedString)
        {
            SceneLoadManager.LoadScene(sharedString.value, LoadSceneMode.Additive);
        }
    }
}