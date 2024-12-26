using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Molca
{
    public abstract class RuntimeSubsystem : MonoBehaviour, IRuntimeSubsystem
    {
        [Flags]
        public enum RuntimeMode
        {
            Nothing = 0,
            Editor = 1 << 0,
            Runtime = 1 << 1
        }

        [FormerlySerializedAs("_mod"), SerializeField]
        private RuntimeMode _runtimeMode;
        private bool IsRuntimeValid { get
            {
                if (Application.isEditor && _runtimeMode.HasFlag(RuntimeMode.Editor))
                    return true;
                else if (!Application.isEditor && _runtimeMode.HasFlag(RuntimeMode.Runtime))
                    return true;
                else 
                    return false;
            } }

        protected bool isActive;
        public bool IsActive => isActive && IsRuntimeValid;

        public abstract void Initialize(Action<IRuntimeSubsystem> finishCallback);
        public virtual void Activate()
        {
            isActive = true;
        }
        public virtual void Deactivate()
        {
            isActive = false;
        }

    }
}