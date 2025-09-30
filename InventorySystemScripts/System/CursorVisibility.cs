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
    }

    private void OnDisable()
    {
        UserInput.OnMouseMovedAction -= ShowCursor;
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

            if (UserInput.UIMoveInput != Vector2.zero)
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
}
