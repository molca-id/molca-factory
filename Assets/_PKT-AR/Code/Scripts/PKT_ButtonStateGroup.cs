using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Molca;

public class PKT_ButtonStateGroup : MonoBehaviour
{
    [SerializeField]
    private bool allowSwitchOff;

    private List<PKT_ButtonState> _buttons = new List<PKT_ButtonState>();

    private IEnumerator Start()
    {
        yield return new WaitUntil(RuntimeManager.IsReady);

        foreach (var e in GetComponentsInChildren<PKT_ButtonState>(true))
            if(!e.exludeFromGroup)
                Register(e);
        EnsureValidState();
    }

    private void Register(PKT_ButtonState buttonState)
    {
        if (_buttons.Contains(buttonState))
            return;

        buttonState.onClicked += NotifyButtonStateChanged;
        _buttons.Add(buttonState);
    }

    private void Unregister(PKT_ButtonState buttonState)
    {
        if (!_buttons.Contains(buttonState))
            return;

        buttonState.onClicked -= NotifyButtonStateChanged;
        _buttons.Remove(buttonState);
    }

    private void NotifyButtonStateChanged(PKT_ButtonState buttonState)
    {
        if(!buttonState.isOn)
        {
            if(!allowSwitchOff)
                buttonState.isOn = true;
            return;
        }

        for (var i = 0; i < _buttons.Count; i++)
        {
            if (_buttons[i] == buttonState)
                continue;

            _buttons[i].isOn = false;
        }
    }

    private void EnsureValidState()
    {
        if (!allowSwitchOff && !AnyButtonOn() && _buttons.Count != 0)
        {
            _buttons[0].isOn = true;
            NotifyButtonStateChanged(_buttons[0]);
        }

        IEnumerable<PKT_ButtonState> activeToggles = ActiveButtons();

        if (activeToggles.Count() > 1)
        {
            PKT_ButtonState firstActive = GetFirstActiveButton();

            foreach (PKT_ButtonState button in activeToggles)
            {
                if (button == firstActive)
                {
                    continue;
                }
                button.isOn = false;
            }
        }
    }

    private bool AnyButtonOn()
    {
        return _buttons.Find(x => x.isOn) != null;
    }

    public IEnumerable<PKT_ButtonState> ActiveButtons()
    {
        return _buttons.Where(x => x.isOn);
    }

    public PKT_ButtonState GetFirstActiveButton()
    {
        IEnumerable<PKT_ButtonState> activeToggles = ActiveButtons();
        return activeToggles.Count() > 0 ? activeToggles.First() : null;
    }

    public void SetAllButtonsOff()
    {
        bool oldAllowSwitchOff = allowSwitchOff;
        allowSwitchOff = true;

        for (var i = 0; i < _buttons.Count; i++)
            _buttons[i].isOn = false;

        allowSwitchOff = oldAllowSwitchOff;
    }
}
