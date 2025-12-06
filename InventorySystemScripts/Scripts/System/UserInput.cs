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

    public static bool IsRadialMenuHeldDown { get; private set; } = false;

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
        BindMouseAction(); // The mouse action reference may lose switched the ActionMap
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
    public void OnRun(InputAction.CallbackContext context) => inputChannel?.RaiseRunEvent(context);
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

    public void OnShowDetails(InputAction.CallbackContext context)
    {
        inputChannel?.RaiseShowDetailsEvent(context);
    }

    public void OnConfirm(InputAction.CallbackContext context)
    {
        if (IsRadialMenuHeldDown) return;
        if (context.started)
        {
            inputChannel?.RaiseConfirmStartedEvent(context);
            inputChannel?.RaiseConfirmEvent(context);   // Old code
        }
        else if (context.performed)
        {
            inputChannel?.RaiseConfirmPerformedEvent(context);
        }
        else if (context.canceled)
        {
            inputChannel?.RaiseConfirmCanceledEvent(context);
        }
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.performed) inputChannel?.RaiseCancelEvent(context);
    }

    public void OnHide(InputAction.CallbackContext context)
    {
        if (IsRadialMenuHeldDown) return;
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

        if ((inputChannel != null && inputChannel.IsInputLocked) && !context.canceled)
        {
            return; 
        }

        // Need to know in a timely manner whether to start opening the menu
        // Prevent executing other commands during the process of opening the menu
        if (context.started)
        {
            IsRadialMenuHeldDown = true;
            inputChannel?.RaiseRadialMenuOpenAnimationEvent();
        }

        if (context.performed)
        {
            inputChannel?.RaiseRadialMenuOpenEvent();
        }
        
        else if (context.canceled)
        {
            IsRadialMenuHeldDown = false;
            inputChannel?.RaiseRadialMenuConfirmEvent();
        }
    }
    #endregion
}
