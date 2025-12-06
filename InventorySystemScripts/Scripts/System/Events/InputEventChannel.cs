using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Description:
// This is an event channel based on ScriptableObject, which focuses on broadcasting user's input events from the new Input System.
// 
// Function:
// Its core function is to act as an "intermediary" to completely decouple UserInput.cs (the source of input signals) from
// all other scripts in the game that need to respond to input (such as MenuController, PlayerController, etc.).
//
// How to use:
// 1. In Project -> Create -> Events -> Input Event Channel
// 2. Drag this instance to the Inspector of UserInput.cs and any other scripts that need to listen for it.
// 3. UserInput calls the "Raise" func to broadcast events
// 4. Other scripts subscribe to these events in the OnEnable() method and unsubscribe from them in the OnDisable() method.

[CreateAssetMenu(menuName = "Events/Input Event Channel")]
public class InputEventChannel : ScriptableObject
{
    // Stores the current lock state directly in the SO.
    // This allows other scripts to check 'IsInputLocked'
    // at any time without needing to track the state manually via event listeners.
    public bool IsInputLocked { get; private set; }

    #region UI Input
    /// <summary>
    /// (WASD, arrow, leftstick: navigate)
    /// </summary>
    public event Action<InputAction.CallbackContext> OnNavigate;

    /// <summary>
    /// (Q: Left) Used for Tab switching
    /// </summary>
    public event Action<InputAction.CallbackContext> OnNavigateLeft;

    /// <summary>
    /// (E: Right) Used for Tab switching
    /// </summary>
    public event Action<InputAction.CallbackContext> OnNavigateRight;

    /// <summary>
    /// Keyboard(J: Confirm)
    /// Gamepad(X: Confirm)
    /// </summary>
    public event Action<InputAction.CallbackContext> OnConfirm;

    /// <summary>
    /// Keyboard(K: Cancel)
    /// Gamepad(O: Confirm)
    /// </summary>
    public event Action<InputAction.CallbackContext> OnCancel;

    /// <summary>
    /// Keyboard(I: Show Details)
    /// Gamepad
    /// </summary>
    public event Action<InputAction.CallbackContext> OnShowDetails;

    /// <summary>
    /// Keyboard(H: Hide the detail panel)
    /// </summary>
    public event Action<InputAction.CallbackContext> OnHide;

    /// <summary>
    /// Keyboard(Space: skip the dialogue)
    /// </summary>
    public event Action<InputAction.CallbackContext> OnSkip;

    /// <summary>
    /// Keyboard(Long press K: Open the radial menu)
    /// </summary>
    public event Action OnRadialMenuOpen;

    /// <summary>
    /// Keyboard(Release J: Select the radial menu item)
    /// </summary>
    public event Action OnRadialMenuConfirm;

    public event Action<InputAction.CallbackContext> OnConfirmStarted;
    public event Action<InputAction.CallbackContext> OnConfirmPerformed;
    public event Action<InputAction.CallbackContext> OnConfirmCanceled;
    #endregion

    #region Player Input

    /// <summary>
    /// Keyboard(WASD: Move)
    /// </summary>
    public event Action<InputAction.CallbackContext> OnMove;

    /// <summary>
    /// Transfer from WALKING
    /// </summary>
    public event Action<InputAction.CallbackContext> OnRun;

    /// <summary>
    /// Keyboard(J: Attack)
    /// </summary>
    public event Action<InputAction.CallbackContext> OnAttack;

    /// <summary>
    /// Keyboard(K: Jump)
    /// </summary>
    public event Action<InputAction.CallbackContext> OnJump;

    #endregion

    // ---- Global Events ----

    /// <summary>
    /// Keyboard(M: Open/Close the menu)
    /// </summary>
    public event Action<InputAction.CallbackContext> OnToggleMenu;
    /// <summary>
    /// MousePosition(Used for UI)
    /// </summary>
    public event Action<Vector2> OnMouseMoved;
    /// <summary>
    /// UserInput Global Lock
    /// </summary>
    public event Action<bool> OnGlobalInputLock;

    // Other Events

    /// <summary>
    /// Keyboard(Keyboard: Start holding K)
    /// </summary>
    public event Action OnRadialMenuHoldStart;

    #region UI Action Events
    public void RaiseNavigateEvent(InputAction.CallbackContext context)                 => OnNavigate?.Invoke(context);
    public void RaiseNavigateLeftEvent(InputAction.CallbackContext context)             => OnNavigateLeft?.Invoke(context);
    public void RaiseNavigateRightEvent(InputAction.CallbackContext context)            => OnNavigateRight?.Invoke(context);
    public void RaiseConfirmEvent(InputAction.CallbackContext context)                  => OnConfirm?.Invoke(context);
    public void RaiseShowDetailsEvent(InputAction.CallbackContext context)              => OnShowDetails?.Invoke(context);
    public void RaiseHideEvent(InputAction.CallbackContext context)                     => OnHide?.Invoke(context);
    public void RaiseSkipEvent(InputAction.CallbackContext context)                     => OnSkip?.Invoke(context);
    public void RaiseRadialMenuOpenEvent()                                              => OnRadialMenuOpen?.Invoke();
    public void RaiseRadialMenuConfirmEvent()                                           => OnRadialMenuConfirm?.Invoke();
    public void RaiseConfirmStartedEvent(InputAction.CallbackContext context)           => OnConfirmStarted?.Invoke(context);
    public void RaiseConfirmPerformedEvent(InputAction.CallbackContext context)         => OnConfirmPerformed?.Invoke(context);
    public void RaiseConfirmCanceledEvent(InputAction.CallbackContext context)          => OnConfirmCanceled?.Invoke(context);
    public void RaiseCancelEvent(InputAction.CallbackContext context)                   => OnCancel?.Invoke(context);
    #endregion

    #region Player Action Events
    public void RaiseMoveEvent(InputAction.CallbackContext context)                     => OnMove?.Invoke(context);
    public void RaiseRunEvent(InputAction.CallbackContext context)                      => OnRun?.Invoke(context);
    public void RaiseJumpEvent(InputAction.CallbackContext context)                     => OnJump?.Invoke(context);
    public void RaiseAttackEvent(InputAction.CallbackContext context)                   => OnAttack?.Invoke(context);
    #endregion

    #region Global Action Events
    public void RaiseToggleMenuEvent(InputAction.CallbackContext context)               => OnToggleMenu?.Invoke(context);
    public void RaiseMouseMovedEvent(Vector2 position)                                  => OnMouseMoved?.Invoke(position);
    public void RaiseGlobalInputLockEvent(bool isLocked) 
    {
        IsInputLocked = isLocked;
        OnGlobalInputLock?.Invoke(isLocked);
    }
    #endregion

    #region Other Action Events
    public void RaiseRadialMenuOpenAnimationEvent()                                     => OnRadialMenuHoldStart?.Invoke();
    #endregion
}
