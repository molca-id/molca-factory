using InteractiveViewer;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[Serializable]
public struct AnimationStateGroup
{
    public string moduleId;
    public PKT_ModuleMaintenance.MaintenanceType type;
    public AnimationState[] states;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onEnter;
    [SerializeField]
    private UnityEvent onExit;

    public void Enter()
    {
        onEnter?.Invoke();
        foreach (var st in states)
        {
            st.Default();
        }
    }

    public void Exit()
    {
        onExit?.Invoke();
    }
}


[Serializable]
public class AnimationState
{
    public enum PlayState
    {
        Start,
        Middle,
        End
    }

    public string name;
    public AnimationStateReference[] stateReferences;

    public Action<AnimationState> onStateStart;
    public Action<AnimationState> onStateEnd;
    public Action<AnimationStateReference> onReferenceChanged;

    private float _speed;

    public bool IsIndexUpdated { get; set; }
    public bool IsValid => stateReferences != null && stateReferences.Length > 0;
    public bool IsPlaying => _speed != 0f;
    public float Speed
    {
        get => _speed;
        set
        {
            _speed = value;
            for (int i = 0; i < stateReferences.Length; i++)
            {
                stateReferences[i].Speed = 0;
            }

            if (_refIndex < stateReferences.Length)
            {
                stateReferences[_refIndex].Speed = value;
            }
        }
    }
    public float Length { get; private set; }
    public PlayState State { get; private set; }

    private int _refIndex;
    public static readonly int DEFAULT_STATE = Animator.StringToHash("default");

    public float Progress
    {
        get{
            if (_refIndex >= stateReferences.Length)
                return 1f;

            //Debug.Log($"State {name} | ref index: {_refIndex}, ref progress: {stateReferences[_refIndex].GetProgress()}");
            //Debug.Log($"State {name} | state progress: {(_refIndex + stateReferences[_refIndex].GetProgress()) / stateReferences.Length}");
            return (_refIndex + stateReferences[_refIndex].GetProgress()) / stateReferences.Length;
        } 
        set
        {
            ProgressUpdate(value, true);
        }
    }

    public IEnumerator Initialize()
    {
        float totalTime = 0f;
        foreach (var state in stateReferences)
        {
            state.Hash = Animator.StringToHash(state.stateName);

            bool goState = state.animator.gameObject.activeSelf;
            state.animator.gameObject.SetActive(true);
            yield return new WaitForEndOfFrame();
            if (state.animator.HasState(state.layer, state.Hash))
            {
                state.SetProgress(0f);
                while(!state.IsStateMatch)
                    yield return new WaitForEndOfFrame();
                totalTime += state.animator.GetCurrentAnimatorStateInfo(state.layer).length;
                state.onFocusChanged += OnReferenceFocusChanged;
            }
            else
            {
                Debug.LogWarning($"State {name} ref {state.stateName} is not a valid state.");
            }
            state.animator.gameObject.SetActive(goState);

            if(state.viewPoint) // set viewpoint to auto attach
                state.viewPoint.autoAttach = true;
        }
        Length = totalTime;
        Pause();
        Default();
        //Debug.Log($"State {name} initialized.");
    }

    private void OnReferenceFocusChanged(AnimationStateReference aniRef)
    {
        if (!IsPlaying)
            return;
        
        if (aniRef.IsFocused)
        {
            GameManager.ActiveModel.GoToViewPoint(aniRef.viewPoint);
            aniRef.onEnter?.Invoke();
        }
        else
        {
            aniRef.onExit?.Invoke();
        }
    }

    public void Play()
    {
        if (!IsValid) return;
        Speed = 1f;
    }

    public void Pause()
    {
        if (!IsValid) return;
        Speed = 0f;
    }

    public void Stop()
    {
        if (!IsValid) return;
        Pause();
        GoToStart();
    }

    public void GoToStart()
    {
        if (!IsValid) return;
        //Debug.Log($"state {name}: GoToStart");
        Progress = 0f;
        onStateStart?.Invoke(this);
    }

    public void GoToEnd()
    {
        if (!IsValid) return;
        //Debug.Log($"state {name}: GoToEnd");
        Progress = 1f;
        onStateEnd?.Invoke(this);
    }

    public void Default()
    {
        if (!IsValid) return;
        Progress = 0f;
        foreach (var state in stateReferences)
        {
            state.Default();
        }
    }

    public void OnUpdate()
    {
        if (IsPlaying) 
            ProgressUpdate(Progress);
    }

    private void ProgressUpdate(float value, bool applyToAnimator = false)
    {
        //Debug.Log($"State {name} | PU | progress set to: {value}");
        int refLength = stateReferences.Length;
        float step = 1f / refLength;
        int id = Mathf.FloorToInt(value / step);
        applyToAnimator = applyToAnimator || id != _refIndex;
        IsIndexUpdated = id != _refIndex;
        /*if (IsIndexUpdated)
            Debug.Log($"State {name} | PU | RefIndex changed, prev: {_refIndex}, current: {id}");*/
        _refIndex = Mathf.Min(id, refLength - 1);

        if(applyToAnimator)
        {
            for (int i = 0; i < _refIndex; i++)
            {
                if (stateReferences[i] == stateReferences[_refIndex])
                    break;
                stateReferences[i].Speed = 0f;
                stateReferences[i].SetProgress(1f);
            }
        }
        //Debug.Log($"State {name} | PU | value: {value}, apply: {applyToAnimator}, index update: {IsIndexUpdated}");
        if (value >= 1f)
        {
            //Debug.Log($"State {name} | PU | value: {value}, apply: {applyToAnimator}");
            IsIndexUpdated = false; // cancel index updated because of the overflow state

            if (State != PlayState.End)
            {
                stateReferences[_refIndex].Speed = Speed;
                stateReferences[_refIndex].SetProgress(1f);

                Pause();
                State = PlayState.End;
                onStateEnd?.Invoke(this);
            }
            return;
        }

        if(applyToAnimator)
        {
            for (int i = refLength - 1; i > _refIndex; i--)
            {
                stateReferences[i].Speed = 0f;
                stateReferences[i].SetProgress(0f);
            }

            //Debug.Log($"State {name} | PU | Reseting state ref {stateReferences[_refIndex].stateName}");
            stateReferences[_refIndex].Speed = Speed;
            stateReferences[_refIndex].SetProgress(value % step * refLength);
        }

        if (value == 0)
        {
            if (State != PlayState.Start)
            {
                State = PlayState.Start;
                onStateStart?.Invoke(this);
            }
        }
        else if (State != PlayState.Middle)
            State = PlayState.Middle;
    }
}

[Serializable]
public class AnimationStateReference
{
    public string stateName;
    public int layer;
    public Animator animator;
    public CameraPoint viewPoint;

    [Header("Events")]
    public UnityEvent onEnter;
    public UnityEvent onExit;

    public Action<AnimationStateReference> onFocusChanged;

    public int Hash { get; set; }
    public float Speed
    {
        get => animator.speed;
        set
        {
            if (animator.speed == value)
                return;

            // Activate the animator before playing
            if (value > 0 && !animator.gameObject.activeSelf)
            {
                Debug.Log($"Activating {animator.name}");
                animator.gameObject.SetActive(true);
            }

            animator.speed = value;
            onFocusChanged?.Invoke(this);

            if (!IsFocused && viewPoint != null && viewPoint.autoAttach)
                viewPoint.Detach();
        }
    }

    public bool IsStateMatch => animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName);
    public bool IsFocused => animator.speed > 0;

    public void Default()
    {
        if (!animator.gameObject.activeSelf) return;

        var speed = animator.speed;
        animator.speed = 1f;
        if (animator.HasState(0, AnimationState.DEFAULT_STATE))
            animator.Play(AnimationState.DEFAULT_STATE, layer, 0f);
        else
            animator.Play(stateName, layer, 0f);
        animator.speed = speed;
    }

    public void SetProgress(float progress)
    {
        if (!animator.gameObject.activeSelf) return;

        // Debug.Log($"State ref {stateName} progress set to: {progress}");
        var speed = animator.speed;
        animator.speed = 1f;
        animator.Play(Hash, layer, progress);
        animator.speed = speed;
    }

    public float GetProgress()
    {
        return IsStateMatch ? animator.GetCurrentAnimatorStateInfo(layer).normalizedTime : 0f;
    }
}
