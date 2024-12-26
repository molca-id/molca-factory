using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Molca
{
    public class SceneLoadManager : RuntimeSubsystem
    {
        private static SceneLoadManager instance;

        public UnityEvent<Scene> onSceneLoaded;
        public UnityEvent<Scene> onSceneUnloaded;
        public UnityEvent<float> onSceneLoadProgress;

        public static Scene activeScene => SceneManager.GetActiveScene();
        private static HashSet<Scene> _loadedScenes;
        private static AsyncOperation _mainOperation;

        public override void Initialize(Action<IRuntimeSubsystem> finishCallback)
        {
            instance = this;
            _loadedScenes = new HashSet<Scene>();

            Activate();
            finishCallback?.Invoke(this);
        }

        public override void Activate()
        {
            base.Activate();

            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        private void SceneLoaded(Scene scn, LoadSceneMode loadMode)
        {
            _loadedScenes.Add(scn);
            onSceneLoaded?.Invoke(scn);
        }

        private void SceneUnloaded(Scene scn)
        {
            _loadedScenes.Remove(scn);
            onSceneUnloaded?.Invoke(scn);
        }

        public static AsyncOperation UnloadScene(string sname)
        {
            var async = SceneManager.UnloadSceneAsync(sname);
            RuntimeManager.RunCoroutine(LoadCoroutine(async));
            return async;
        }

        public static AsyncOperation LoadScene(string sname, LoadSceneMode mode = LoadSceneMode.Single)
        {
            var async = SceneManager.LoadSceneAsync(sname, mode);
            RuntimeManager.RunCoroutine(LoadCoroutine(async));
            return async;
        }

        public static AsyncOperation LoadNextScene(LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!Application.isPlaying) return null;

            var async = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1, mode);
            RuntimeManager.RunCoroutine(LoadCoroutine(async));
            return async;
        }

        public static bool IsSceneLoaded(string sname)
        {
            foreach (var e in _loadedScenes)
                if (e.name == sname)
                    return true;
            return false;
        }

        private static IEnumerator LoadCoroutine(AsyncOperation async)
        {
            while(!async.isDone)
            {
                yield return new WaitForEndOfFrame();
                if (_mainOperation == async)
                    instance.onSceneLoadProgress?.Invoke(async.progress);
                else if (_mainOperation == null)
                    _mainOperation = async;
            }
            if (_mainOperation == async)
                _mainOperation = null;
        }
    }
}