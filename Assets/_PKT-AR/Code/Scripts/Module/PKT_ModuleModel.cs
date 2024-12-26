using Molca;
using Molca.Modals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace InteractiveViewer
{
    [RequireComponent(typeof(AnimatorHelper))]
    public class PKT_ModuleModel : MonoBehaviour
    {
        [Serializable]
        internal struct RootInfo : IEquatable<RootInfo>
        {
            public string moduleId;
            public Transform rootTransform;
            public CameraPoint defaultViewPoint;
            public GameObject[] destroyOnInitiate;

            public bool IsValid => rootTransform != null && defaultViewPoint != null;

            public bool Equals(RootInfo other)
            {
                return moduleId == other.moduleId;
            }

            public static RootInfo GetDefault()
            {
                return new RootInfo()
                {
                    moduleId = null,
                    rootTransform = GameManager.ActiveModel.transform,
                    defaultViewPoint = GameManager.ActiveModel.GetPointByName(DEFAULT_VIEW_POINT),
                    destroyOnInitiate = null
                };
            }
        }

        [Serializable]
        internal class MeshHighlightInfo
        {
            public string name;
            [FormerlySerializedAs("pivot")]
            public CameraPoint viewPoint;
            public Transform[] targets;

            public HashSet<Renderer> Renderers { get; set; }

            public void ToggleRenderer(bool value)
            {
                foreach (var r in Renderers)
                    r.enabled = value;
            }

            public bool CompareName(string other)
            {
                if (other == null) return false;

                ReadOnlySpan<char> thisSpan = name.AsSpan().Trim();
                ReadOnlySpan<char> otherSpan = other.AsSpan().Trim();

                return thisSpan.Equals(otherSpan, StringComparison.OrdinalIgnoreCase);
            }
        }

        [SerializeField]
        internal GameObject environment;
        [SerializeField]
        internal Bounds environmentBounds;
        [SerializeField]
        internal RootInfo[] moduleRoots;
        [SerializeField]
        internal MeshHighlightInfo[] components;

        public const string DEFAULT_VIEW_POINT = "default";

        private CameraPoint[] _viewPoints;
        private CameraController _camController;
        private Dictionary<MeshRenderer, List<Material>> _cachedMaterials;

        public Action onResetView;

        internal RootInfo ActiveRoot { get; private set; }
        public bool IsReady { get; private set; }
        public bool PreviewMode { get; private set; }
        public AnimatorHelper AnimHelper { get; private set; }

        public IEnumerator Initialize()
        {
            _camController = FindFirstObjectByType<CameraController>();
            if (environmentBounds.size.magnitude > 10) // Check if bounding box is set up
                _camController.targetBounds = environmentBounds;

            ActiveRoot = moduleRoots.First(mr => mr.moduleId == GameManager.ActiveModule.id);
            foreach (var go in ActiveRoot.destroyOnInitiate)
                if (go != null)
                    Destroy(go);

            _viewPoints = GetComponentsInChildren<CameraPoint>(true);

            // Activate module roots one by one
            if (environment != null)
                yield return ProgressiveRootActivation(environment.transform, true);

            foreach (var mr in moduleRoots)
            {
                yield return new WaitForEndOfFrame();
                foreach (var anim in mr.rootTransform.GetComponentsInChildren<Animator>(true))
                {
                    if (mr.Equals(ActiveRoot))
                        anim.speed = 0f;
                    else
                        Destroy(anim);
                }

                yield return ProgressiveRootActivation(mr.rootTransform, true, 16);
            }

            if (!ActiveRoot.IsValid)
            {
                ActiveRoot = RootInfo.GetDefault();
                Debug.LogWarning("Active root invalid, using default.");
            }

            AnimHelper = GetComponent<AnimatorHelper>();
            yield return AnimHelper.Initialize();

            string loadingKey = "Preparing model data ";
            var renderers = ActiveRoot.rootTransform.GetComponentsInChildren<MeshRenderer>(true);
            int loadCount = 0;
            int chunk = 0;
            float loadLength = (renderers.Length + components.Sum(e => e.targets.Length)) / 30f;
            var loading = RuntimeManager.GetSubsystem<ModalManager>().AddLoading(loadingKey);

            // Cache non-environment's renderer's materials
            _cachedMaterials = new Dictionary<MeshRenderer, List<Material>>();
            foreach (var r in renderers)
            {
                _cachedMaterials.Add(r, r.sharedMaterials.ToList());
                chunk++;
                if(chunk == 30)
                {
                    chunk = 0;
                    yield return new WaitForEndOfFrame();
                    loadCount++;
                    loading.Refresh($"{loadingKey} <b>({loadCount / loadLength:P})", loadCount / loadLength);
                }
            }

            // Initialize mesh highlight infos
            for (int p = 0; p < components.Length; p++)
            {
                components[p].Renderers = new HashSet<Renderer>();
                for (int t = 0; t < components[p].targets.Length; t++)
                {
                    if (components[p].targets[t] == null)
                        continue;
                    foreach(var r in components[p].targets[t].GetComponentsInChildren<MeshRenderer>())
                        components[p].Renderers.Add(r);
                    //Debug.Log($"{components[p].name} renderer count: {components[p].Renderers.Count}");
                    chunk++;
                    if (chunk == 30)
                    {
                        chunk = 0;
                        yield return new WaitForEndOfFrame();
                        loadCount++;
                        loading.Refresh($"{loadingKey} <b>({loadCount / loadLength:P})", loadCount / loadLength);
                    }
                }
            }
            loading.Refresh($"{loadingKey} ({1f:P})", 1f);

            yield return new WaitForSeconds(.2f);
            RuntimeManager.GetSubsystem<ModalManager>().RemoveLoading(loadingKey);
            ResetViewPoint();

            IsReady = true;
        }

        public bool HasPoint(string pName)
        {
            if(string.IsNullOrEmpty(pName)) return false;
            for (int i = 0; i < _viewPoints.Length; i++)
            {
                if (_viewPoints[i].PointName == pName)
                    return true;
            }
            return false;
        }

        public CameraPoint GetPointByIndex(int id)
        {
            if (id < 0 || id >= _viewPoints.Length)
                return null;

            return _viewPoints[id];
        }

        public CameraPoint GetPointByName(string pointName)
        {
            if (string.IsNullOrEmpty(pointName)) return null;
            for (int i = 0; i < _viewPoints.Length; i++)
            {
                if (_viewPoints[i].PointName == pointName)
                    return _viewPoints[i];
            }
            return null;
        }

        public void GoToViewPoint(string pointName)
        {
            GoToViewPoint(GetPointByName(pointName));
        }

        public void GoToViewPoint(CameraPoint point)
        {
            //Debug.Log($"Go to view point: {point}");
            if (point != null)
                _camController.MoveToPoint(point);
        }

        public void ResetLocalTransform()
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
        }

        public void ResetViewPoint()
        {
            if (_camController == null)
            {
                throw new Exception("Camera Controller is null.");
            }

            _camController.MoveToPoint(ActiveRoot.defaultViewPoint ? 
                                        ActiveRoot.defaultViewPoint : GetPointByName(DEFAULT_VIEW_POINT));
        }

        public void SetPreviewMode(bool value)
        {
            if (value == PreviewMode)
                return;

            PreviewMode = value;
            int matCount = 0;
            foreach (var r in _cachedMaterials.Keys)
            {
                if (PreviewMode != value)
                    return;
                if (PreviewMode)
                {
                    var mats = new List<Material>();
                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                        mats.Add(GameManager.PreviewMaterial);
                    r.SetSharedMaterials(mats);
                }
                else
                {
                    r.SetSharedMaterials(_cachedMaterials[r]);
                }
                matCount += r.sharedMaterials.Length;
            }
        }

        public void ToggleEnvironment(bool value)
        {
            if (environment) 
                environment.SetActive(value);
        }

        /// <summary>
        /// Empty or invalid name will reset component
        /// </summary>
        /// <param name="key"></param>
        public void SelectComponent(string key = "")
        {
            MeshHighlightInfo component = null;
            if(!string.IsNullOrEmpty(key))
            {
                for (int i = 0; i < components.Length; i++)
                {
                    if (!components[i].CompareName(key))
                        continue;

                    component = components[i];
                    break;
                }
            }

            if (component == null && !string.IsNullOrEmpty(key))
                Debug.LogWarning($"No component with key: {key}");

            foreach (var r in _cachedMaterials.Keys)
            {
                bool focused = component != null ? component.Renderers.Contains(r) : true;
                if (!focused)
                {
                    var mats = new List<Material>();
                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                        mats.Add(GameManager.PreviewMaterial);
                    r.SetSharedMaterials(mats);
                }
                else
                {
                    r.SetSharedMaterials(_cachedMaterials[r]);
                }
                //r.enabled = component != null ? component.Renderers.Contains(r) : true;
            }
            _camController.MoveToPoint(component != null ? component.viewPoint : null);
        }

        public IEnumerator ProgressiveRootActivation(Transform parent, bool value, int chunkSize = 4)
        {
            var childs = parent.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.gameObject.activeSelf)
                        .Select(t => t.gameObject).ToArray();

            foreach (GameObject child in childs)
                child.SetActive(false);
            parent.gameObject.SetActive(value);

            if (!value)
                yield break;

            int chunk = 0;
            int loadCount = 0;
            float loadLength = childs.Length / chunkSize;

            string loadingKey = $"Loading: {parent.name}";
            var loading = RuntimeManager.GetSubsystem<ModalManager>().AddLoading(loadingKey);
            foreach (GameObject child in childs)
            {
                chunk++;
                if (chunk == chunkSize)
                {
                    loadCount++;
                    loading.Refresh($"{loadingKey} ({loadCount / loadLength:P})", loadCount / loadLength);
                    chunk = 0;
                    yield return new WaitForEndOfFrame();
                }
                child.SetActive(true);
            }
            RuntimeManager.GetSubsystem<ModalManager>().RemoveLoading(loadingKey);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(environmentBounds.center, environmentBounds.size);
        }
    }
}