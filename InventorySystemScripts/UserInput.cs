using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TabsManager tabsManager;

    public static PlayerInput playerInput;

    public static Vector2 MoveInput;
    public static Vector2 MousePos;
    public static Vector2 UIMoveInput;

    // Player Actions
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;

    // UI Actions
    private InputAction _navigateAction;
    private InputAction _navigateLeftAction;
    private InputAction _navigateRightAction;
    private InputAction _confirmAction;
    private InputAction _showDetailsAction;
    private InputAction _hideAction;

    // Global Actions
    private InputAction _toggleMenuAction;
    private InputAction _mouseAction;

    private Vector2 _lastMousePos;      // track last mouse position

    public delegate void MouseMoveAction();
    public static event MouseMoveAction OnMouseMovedAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        SetupActions();
    }

    private void OnEnable()
    {
        BindGlobalActions();

        if (playerInput.currentActionMap.name == "UI")
            BindUIActions();
        else
            BindPlayerActions();
    }

    private void Start()
    {
        playerInput.SwitchCurrentActionMap("Player");
        // RebindActions();
    }

    private void Update()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();
        MousePos = _mouseAction.ReadValue<Vector2>();

        if (MousePos != _lastMousePos)
        {
            MouseMovedEvent();
        }

        _lastMousePos = MousePos;

        // Debug.Log($"MousePos: {MousePos}");
    }

    private void OnDisable()
    {
        UnbindGlobalActions();
        UnbindPlayerActions();
        UnbindUIActions();
    }

    // When adding new actions, map it to the actions in the new Input System
    // For global operations, place the configuration in the Global binding function.
    private void SetupActions()
    {
        // Player
        _moveAction                      = playerInput.actions["Move"];
        _jumpAction                      = playerInput.actions["Jump"];
        _attackAction                    = playerInput.actions["Attack"];

        // UI
        _navigateAction                  = playerInput.actions["Navigate"];
        _navigateLeftAction              = playerInput.actions["NavigateLeft"];
        _navigateRightAction             = playerInput.actions["NavigateRight"];
        _confirmAction                   = playerInput.actions["Confirm"];
        _showDetailsAction               = playerInput.actions["ShowDetails"];
        _hideAction                      = playerInput.actions["Hide"];

    }

    #region [SwitchToPlayer] & [SwitchToUI] used to change action map
    // When resuming game, switch current action map to Player
    public void SwitchToPlayer()
    {
        UnbindUIActions();
        playerInput.SwitchCurrentActionMap("Player");
        BindPlayerActions();

        BindGlobalActions();
    }

    // When opening UI interface, switch current action map to UI
    public void SwitchToUI()
    {
        UnbindPlayerActions();
        playerInput.SwitchCurrentActionMap("UI");
        BindUIActions();

        BindGlobalActions();
    }
    #endregion

    /// <summary>
    /// When adding new inputs, place them in the bind function below.
    /// </summary>
    #region [Global]
    private void BindGlobalActions()
    {
        _toggleMenuAction               = playerInput.actions["ToggleMenu"];
        _mouseAction                    = playerInput.actions["MousePosition"];
        _toggleMenuAction.performed     += OnToggleMenu;
    }

    private void UnbindGlobalActions()
    {
        _toggleMenuAction.performed     -= OnToggleMenu;
    }
    #endregion
    #region [Player]
    private void BindPlayerActions()
    {
        _jumpAction.performed           += OnJump;
        _attackAction.performed         += OnAttack;
    }

    private void UnbindPlayerActions()
    {
        _jumpAction.performed           -= OnJump;
        _attackAction.performed         -= OnAttack;
    }
    #endregion
    #region [UI]
    private void BindUIActions()
    {
        _navigateAction.performed       += OnNavigate;
        _navigateLeftAction.performed   += OnNavigateLeft;
        _navigateRightAction.performed  += OnNavigateRight;
        _confirmAction.performed        += OnConfirm;
        _showDetailsAction.performed    += OnShowDetails;
        _hideAction.performed           += OnHide;
    }

    private void UnbindUIActions()
    {
        if(_navigateAction != null) _navigateAction.performed               -= OnNavigate;
        if (_navigateLeftAction != null) _navigateLeftAction.performed      -= OnNavigateLeft;
        if (_navigateRightAction != null) _navigateRightAction.performed    -= OnNavigateRight;
        if (_confirmAction != null) _confirmAction.performed                -= OnConfirm;
        if (_showDetailsAction != null) _showDetailsAction.performed        -= OnShowDetails;
        if (_hideAction != null) _hideAction.performed                      -= OnHide;
    }
    #endregion


    /// <summary>
    /// The following is the input callback method bound through the InputAction callback
    /// Used to respond to and process player input
    /// </summary>
    #region [Input Actions]
    private void OnNavigate(InputAction.CallbackContext obj)
    {
        UIMoveInput = obj.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext obj)
    {
        Debug.Log("Jump!");
        // TODO: jump logic
    }

    private void OnAttack(InputAction.CallbackContext obj)
    {
        Debug.Log("Attack!");
        // TODO: attack logic
    }

    private void OnHide(InputAction.CallbackContext obj)
    {
        Debug.Log("Hide");
        TooltipInstance.instance.ToggleTooltip();
    }

    private void OnShowDetails(InputAction.CallbackContext obj)
    {
        Debug.Log("Show details");
        DetailsPanel.instance.ChangePanel();
    }

    private void OnConfirm(InputAction.CallbackContext obj)
    {
        Debug.Log("Confirm");
    }

    private void OnToggleMenu(InputAction.CallbackContext obj)
    {
        if (!MenuController.instance.IsMenuOpen)
        {
            MenuController.instance.ToggleMenu();
            SwitchToUI();
        }
        else
        {
            MenuController.instance.ToggleMenu();
            SwitchToPlayer();
        }
    }

    public void OnNavigateLeft(InputAction.CallbackContext obj)
    {
        if (tabsManager != null && MenuController.instance.IsMenuOpen)
        {
            tabsManager.NavigateTabs(-1);
        }
    }

    public void OnNavigateRight(InputAction.CallbackContext obj)
    {
        if (tabsManager != null && MenuController.instance.IsMenuOpen)
        {
            tabsManager.NavigateTabs(1);
        }
    }

    public void MouseMovedEvent()
    {
        OnMouseMovedAction?.Invoke();
    }
    #endregion
}
