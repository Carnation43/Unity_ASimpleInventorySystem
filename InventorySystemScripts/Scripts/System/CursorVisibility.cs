using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorVisibility : MonoBehaviour
{
    private bool firstMenuOpen = true;

    [Header("Listening To")]
    [SerializeField] private InputEventChannel inputChannel;

    [SerializeField] private MenuController menu;

    private void OnEnable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnMouseMoved += HandleMouseMoved;
        }
    }

    private void OnDisable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnMouseMoved -= HandleMouseMoved;
        }
    }

    private void HandleMouseMoved(Vector2 position)
    {
        ShowCursor();
    }

    private void Update()
    {

        if (menu.IsMenuOpen && menu != null)
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
