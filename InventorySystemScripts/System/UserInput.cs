using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class UserInput : MonoBehaviour
{
    public static UserInput instance;

    [Header("Broadcasting On")]
    [SerializeField] private InputEventChannel inputChannel;

    public static PlayerInput playerInput;
    public static Vector2 UIMoveInput;

    private Vector2 _lastMousePos;      // track last mouse position
    private InputAction _mouseAction;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) { Destroy(gameObject); return; }
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        playerInput.SwitchCurrentActionMap("Player");
        // RebindActions();
        BindMouseAction();
    }

    private void Update()
    {
        if (_mouseAction != null)
        {
            Vector2 currentMousePos = _mouseAction.ReadValue<Vector2>();
            if (currentMousePos != _lastMousePos)
            {
                inputChannel?.RaiseMouseMovedEvent(currentMousePos);
            }
            _lastMousePos = currentMousePos;
        }
    }

    public void SwitchActionMap(string mapName)
    {
        playerInput.SwitchCurrentActionMap(mapName);
        Debug.Log($"Input Action Map Switched to: {mapName}");

        BindMouseAction();
    }

    private void BindMouseAction()
    {
        _mouseAction = playerInput.actions["MousePosition"];
    }

    /// <summary>
    /// The following is the input callback method bound through the InputAction callback
    /// Used to respond to and process player input
    /// </summary>
    #region [Input Actions Callbacks]
    public void OnMove(InputAction.CallbackContext context) => inputChannel?.RaiseMoveEvent(context);
    public void OnJump(InputAction.CallbackContext context) => inputChannel?.RaiseJumpEvent(context);
    public void OnAttack(InputAction.CallbackContext context) => inputChannel?.RaiseAttackEvent(context);

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            UIMoveInput = Vector2.zero;
        }
        else
        {
            UIMoveInput = context.ReadValue<Vector2>();
        }
        
        inputChannel?.RaiseNavigateEvent(context);
    }
    public void OnNavigateLeft(InputAction.CallbackContext context)
    {
        if (context.performed) inputChannel?.RaiseNavigateLeftEvent(context);
    }

    public void OnNavigateRight(InputAction.CallbackContext context)
    {
        if (context.performed) inputChannel?.RaiseNavigateRightEvent(context);
    }

    public void OnConfirm(InputAction.CallbackContext context)
    {
        if (context.performed) inputChannel?.RaiseConfirmEvent(context);
    }

    public void OnHide(InputAction.CallbackContext context)
    {
        if (context.performed) inputChannel?.RaiseHideEvent(context);
    }

    public void OnSkip(InputAction.CallbackContext context)
    {
        if (context.performed) inputChannel?.RaiseSkipEvent(context);
    }

    public void OnToggleMenu(InputAction.CallbackContext context)
    {
        if (context.started) inputChannel?.RaiseToggleMenuEvent(context);
    }

    public void OnRadialMenu(InputAction.CallbackContext context)
    {
        // 当按住时间满足Hold Interaction的要求时，context.performed为true
        if (context.performed)
        {
            inputChannel?.RaiseRadialMenuOpenEvent();
        }
        // 当按键松开时，context.canceled为true
        else if (context.canceled)
        {
            inputChannel?.RaiseRadialMenuConfirmEvent();
        }
    }
    #endregion
}
