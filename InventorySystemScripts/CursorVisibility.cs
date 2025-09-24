using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorVisibility : MonoBehaviour
{

    private bool firstMenuOpen = true;

    private void OnEnable()
    {
        UserInput.OnMouseMovedAction += ShowCursor;
        // InputSystem.onActionChange += OnInputActionChange;
    }

    private void OnDisable()
    {
        UserInput.OnMouseMovedAction -= ShowCursor;
        // InputSystem.onActionChange -= OnInputActionChange;
    }

    private void Update()
    {
        if (MenuController.instance.IsMenuOpen)
        {
            if (firstMenuOpen)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
                firstMenuOpen = false;
            }

            if (UserInput.MoveInput != Vector2.zero)
            {
                HideCursor();
            }
        }
        else
        {
            firstMenuOpen = true;
        }
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //// test
    //private void OnInputActionChange(object obj, InputActionChange change) {
    //    if(change == InputActionChange.ActionPerformed)
    //    {
    //        InputAction inputAction = (InputAction)obj;
    //        InputControl lastControl = inputAction.activeControl;
    //        InputDevice lastDevice = lastControl.device;

    //        Debug.Log("lastDevice:" + lastDevice.name);

    //        if(lastDevice.displayName == "Mouse")
    //        {
    //            Cursor.visible = true;
    //        }
    //        else
    //        {
    //            Cursor.visible = false;
    //        }
    //    }
    //}
}
