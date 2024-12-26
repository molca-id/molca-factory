using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using InteractiveViewer;
using UnityEngine.Events;
using Molca.Modals;
using Molca;

public class AnimatorHelper : MonoBehaviour
{
    [SerializeField]
    private AnimationStateGroup[] stateGroups;
    private float _normalizedTime;

    [Header("Events")]
    public UnityEvent onReset;

    public Action<string> onAnimStateChanged;
    public Action<float> onAnimProgress;
    public Action<bool> onPlayStateChanged;

    private int _groupIndex;
    private bool _isAutoPlaying;
    private bool _stateChanged;
    private float _lastUpdate;
    private readonly float _updateInterval = .05f;

    private AnimationState[] FilteredState => (stateGroups.Length > 0 && _groupIndex >= 0) ? stateGroups[_groupIndex].states : null;
    public AnimationState ActiveState { get; private set; }
    public bool IsLooping { get; set; } = false;
    public bool IsPlaying => ActiveState != null && ActiveState.IsPlaying;
    public bool AutoPlay
    {
        get
        {
            return _isAutoPlaying;
        }
        set
        {
            _isAutoPlaying = value;
            if(value == true)
                SetState(FilteredState[0]);
        }
    }
    public float ConsolidatedProgress
    {
        get
        {
            return (float)(FilteredState.Where(s => s.State == AnimationState.PlayState.End).Count() + ActiveState.Progress) / FilteredState.Length;
        }
        set
        {
            value = Mathf.Clamp(value, 0f, .9999f); // HACK: Setting the value to 1 will set the progress to the begining of the last state.
            float step = 1f / FilteredState.Length;
            int id = Mathf.FloorToInt(value / step);
            if(id < FilteredState.Length && ActiveState != FilteredState[id])
                SetState(FilteredState[id], true);
            ActiveState.Progress = value % step * FilteredState.Length;
        }
    } // need to set all states progress

    public IEnumerator Initialize()
    {
        foreach (var e in GetComponentsInChildren<Animator>(true))
            e.speed = 0f;

        string loadingKey = "Initialize animation state: ";
        for (int i = stateGroups.Length - 1; i >= 0; i--) {
            AnimationStateGroup sg = stateGroups[i];
            if(sg.moduleId != GameManager.ActiveModule.id)
                continue; // Only initialize groups with same module ID

            var loading = RuntimeManager.GetSubsystem<ModalManager>().AddLoading(loadingKey + sg.type);
            float progress = 0;
            foreach (var state in sg.states)
            {
                yield return state.Initialize();
                state.onStateEnd += OnStateEnd;

                progress += 1;
                loading.Refresh($"{loadingKey}{sg.type} ({progress/sg.states.Length:P})", progress / sg.states.Length);
            }
            RuntimeManager.GetSubsystem<ModalManager>().RemoveLoading(loadingKey + sg.type);
        }
    }

    private void OnStateEnd(AnimationState state)
    {
        if(ActiveState != state) return;

        if (!AutoPlay)
        {
            if (IsLooping)
                SetState(state, true);
            else
                onPlayStateChanged?.Invoke(false);
        }
        else
        {
            int nextIndex = Array.IndexOf(FilteredState, state) + 1;
            if (nextIndex < FilteredState.Length)
                SetState(FilteredState[nextIndex], true);
            else if (IsLooping)
            {
                stateGroups[_groupIndex].Enter();
                SetState(FilteredState[0], true);
            }
            else
                onPlayStateChanged?.Invoke(false);
        }
    }

    public void ResumeAnimation()
    {
        if (ActiveState != null)
        {
            ActiveState.Play();
            onPlayStateChanged?.Invoke(true);
        }
    }

    public void PauseAnimation()
    {
        if(AutoPlay)
        {
            foreach (var state in FilteredState)
                state.Pause();
        }
        else if (ActiveState != null)
        {
            ActiveState.Pause();
        }
        onPlayStateChanged?.Invoke(false);
    }

    public void FilterStates(PKT_ModuleMaintenance.MaintenanceType type)
    {
        string moduleId = GameManager.ActiveModule.id;
        for (int i = 0; i < stateGroups.Length; i++)
        {
            if (stateGroups[i].type == type && stateGroups[i].moduleId == moduleId)
            {
                _groupIndex = i;
                stateGroups[i].Enter();
                return;
            }
        }
        Debug.LogWarning($"No state group of type {type.ToString()} in module: {moduleId}");
    }

    public void SetState(string stateName, bool resume = false)
    {
        SetState(GetStateByName(stateName), resume);
    }

    public void SetState(AnimationState state, bool resume = false)
    {
        if (IsPlaying) PauseAnimation();

        ActiveState = state;
        if (state == null)
        {
            onAnimStateChanged?.Invoke("");
            return;
        }

        for (int i = 0; i < FilteredState.Length; i++)
        {
            if (FilteredState[i] == state)
                break;
            FilteredState[i].GoToEnd();
        }

        for (int i = FilteredState.Length - 1; i >= 0; i--)
        {
            if (FilteredState[i] == state)
                break;
            FilteredState[i].Default();
        }

        if (resume)
        {
            // supposed to yield here
            ResumeAnimation();
        }

        ActiveState.Progress = 0f;
        onAnimStateChanged?.Invoke(state.name);

        _stateChanged = true;
    }

    public void SetAnimationProgress(float progress)
    {
        if(!AutoPlay && ActiveState != null)
        {
            ActiveState.Progress = progress;
        }
        else
        {
            ConsolidatedProgress = progress;
        }
        _lastUpdate = Time.time;
    }

    public void SetStateProgress(string stateName, float progress)
    {
        var state = GetStateByName(stateName);
        if(state == null) return;
        state.Progress = 0;
    }

    public void ResetState(string stateName)
    {
        var state = GetStateByName(stateName);
        if (state == null) return;
        state.Default();
    }

    public void ResetAll()
    {
        PauseAnimation();

        if (_groupIndex >= 0)
            stateGroups[_groupIndex].Exit();

        for (int i = stateGroups.Length - 1; i >= 0; --i)
        {
            if (stateGroups[i].moduleId != GameManager.ActiveModule.id)
                continue;
            for(int j = stateGroups[i].states.Length - 1; j >= 0; j--)
            {
                stateGroups[i].states[j].Default();
            }
        }
        _groupIndex = -1;
        ActiveState = null;

        onReset?.Invoke();
        //Debug.Log("Animator states is reset.");
    }

    private void LateUpdate()
    {
        if (!IsPlaying || Time.time - _lastUpdate < _updateInterval)
            return;

        if(_stateChanged)
            _stateChanged = false;
        else
        {
            for (int i = 0; i < FilteredState.Length; i++)
            {
                FilteredState[i].OnUpdate();
            }
        }

        if(!ActiveState.IsIndexUpdated)
            onAnimProgress?.Invoke(_isAutoPlaying ? ConsolidatedProgress : ActiveState.Progress);
        _lastUpdate = Time.time;
    }

    public bool HasState(string stateName)
    {
        if (string.IsNullOrEmpty(stateName) || FilteredState == null) return false;
        for (int i = 0; i < FilteredState.Length; i++)
        {
            if (FilteredState[i].name == stateName)
                return true;
        }
        return false;
    }

    public AnimationState GetStateByName(string sName)
    {
        if(string.IsNullOrEmpty(sName)) return null;
        for (int i = 0; i < FilteredState.Length; i++)
        {
            if (FilteredState[i].name == sName)
                return FilteredState[i];
        }
        return null;
    }

    [Button]
    public void ValidateID()
    {
        foreach (var sg in stateGroups)
        {
            char idPrefix = '-';
            switch (sg.type)
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

            foreach (var state in sg.states)
            {
                if (idPrefix == '-' || state.name.Length == 0 || state.name[0] == idPrefix)
                    continue;

                state.name = state.name.Insert(0, $"{idPrefix}");
                Debug.Log($"Added prefix '{idPrefix}' to state: {state.name}");
            }
        }
    }
}
