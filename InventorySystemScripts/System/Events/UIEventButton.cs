using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum ButtonActionType
{
    None,
    Confirm,
    ShowDetails,
    Hide,
    Skip,
    NavigateLeft,
    NavigateRight
}

[RequireComponent(typeof(Button))]
public class UIEventButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Broadcast On")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("Button Settings")]
    [Tooltip("Specify the action type when the button is triggered")]
    [SerializeField] private ButtonActionType actionType = ButtonActionType.None;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

  
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (actionType)
        {
            case ButtonActionType.Confirm:
                inputChannel?.RaiseConfirmEvent(default);
                break;
            case ButtonActionType.Hide:
                inputChannel?.RaiseHideEvent(default);
                break;
            //case ButtonActionType.ShowDetails:
            //    inputChannel?.RaiseShowDetailsEvent(default);
            //    break;
            case ButtonActionType.Skip:
                inputChannel?.RaiseSkipEvent(default);
                break;
        }
    }
}
