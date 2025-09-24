using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 MoveInput;
    public static Vector2 MousePos;

    private InputAction _moveAction;
    private InputAction _mouseAction;

    private Vector2 _lastMousePos;      // track last mouse position

    public delegate void MouseMoveAction();
    public static event MouseMoveAction OnMouseMovedAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _mouseAction = PlayerInput.actions["MousePosition"];
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

    public void MouseMovedEvent()
    {
        OnMouseMovedAction?.Invoke();
    }
}
