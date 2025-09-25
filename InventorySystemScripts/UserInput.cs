using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TabsManager tabsManager;

    public static PlayerInput PlayerInput;

    public static Vector2 MoveInput;
    public static Vector2 MousePos;

    private InputAction _moveAction;
    private InputAction _mouseAction;
    private InputAction _navigateLeftAction;
    private InputAction _navigateRightAction;

    private Vector2 _lastMousePos;      // track last mouse position

    public delegate void MouseMoveAction();
    public static event MouseMoveAction OnMouseMovedAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _mouseAction = PlayerInput.actions["MousePosition"];
        _navigateLeftAction = PlayerInput.actions["NavigateLeft"];
        _navigateRightAction = PlayerInput.actions["NavigateRight"];
    }

    private void Update()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();
        MousePos = _mouseAction.ReadValue<Vector2>();

        if(MousePos != _lastMousePos)
        {
            MouseMovedEvent();
        }

        _lastMousePos = MousePos;
        
    }

    private void OnEnable()
    {
        _navigateLeftAction.performed += OnNavigateLeftPerformed;
        _navigateLeftAction.Enable();
        _navigateRightAction.performed += OnNavigateRightPerformed;
        _navigateRightAction.Enable();
    }

    private void OnDisable()
    {
        _navigateLeftAction.performed -= OnNavigateLeftPerformed;
        _navigateLeftAction.Disable();
        _navigateRightAction.performed -= OnNavigateRightPerformed;
        _navigateRightAction.Disable();
    }

    public void OnNavigateLeftPerformed(InputAction.CallbackContext obj)
    {
        if (tabsManager != null && MenuController.instance.IsMenuOpen)
        {
            tabsManager.NavigateTabs(-1);
        }
    }

    public void OnNavigateRightPerformed(InputAction.CallbackContext obj)
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


}
