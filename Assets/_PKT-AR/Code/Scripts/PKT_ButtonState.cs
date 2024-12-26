using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Molca;

[RequireComponent(typeof(Button))]
public class PKT_ButtonState : MonoBehaviour
{
    public bool exludeFromGroup;
    [SerializeField]
    private bool _isOn;
    [SerializeField]
    private TextMeshProUGUI labelText;
    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private bool interpolateColor;
    [SerializeField]
    private Color onColor;
    [SerializeField]
    private Color offColor;

    [SerializeField]
    private bool toggleBackground;
    [SerializeField]
    private Sprite onSprite;
    [SerializeField]
    private Sprite offSprite;

    public UnityEvent<bool> onStateChanged;
    public UnityEvent onStateOn;
    public UnityEvent onStateOff;

    public Action<PKT_ButtonState> onClicked;

    public bool isOn
    {
        get => _isOn;
        set
        {
            //Debug.Log($"{gameObject.name} isOn: {value}");
            _isOn = value;
            if (labelText)
                labelText.color = isOn ? onColor : offColor;
            if (toggleBackground)
            {
                if (onSprite)
                    backgroundImage.sprite = isOn ? onSprite : offSprite;
                else
                    RuntimeManager.RunCoroutine(SetColor(isOn ? onColor : offColor));
            }
            InvokeStateEvent();
        }
    }

    private IEnumerator Start()
    {
        //Debug.Log($"{gameObject.name} => Waiting RM Ready.");
        yield return new WaitUntil(RuntimeManager.IsReady);

        //yield return new WaitUntil(RuntimeManager.IsReady); BUG: Home's ListView Toggle don't go pass this line
        isOn = isOn;
        GetComponent<Button>().onClick.AddListener(OnClicked);
        //Debug.Log($"{gameObject.name} => RM Ready.");
    }

    private void OnClicked()
    {
        isOn = !isOn;
        onClicked?.Invoke(this);
    }

    /// <summary>
    /// Use this to notify the state group
    /// </summary>
    /// <param name="value"></param>
    public void SetState(bool value)
    {
        if (isOn == value) return;

        isOn = value;
        onClicked?.Invoke(this);
    }

    public void InvokeStateEvent()
    {
        onStateChanged?.Invoke(isOn);
        if (isOn)
            onStateOn?.Invoke();
        else
            onStateOff?.Invoke();
    }

    private IEnumerator SetColor(Color target)
    {
        if(!interpolateColor)
        {
            backgroundImage.color = target;
            yield break;
        }

        Color start = backgroundImage.color;
        float a = 0f;
        while(a < 1f)
        {
            a += Time.deltaTime * 5f;
            backgroundImage.color = Color.Lerp(start, target, a);
            yield return new WaitForEndOfFrame();
        }
    }
}