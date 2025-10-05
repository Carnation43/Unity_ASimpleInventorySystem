using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the user input and logic for the radial menu.
/// </summary>
public class RadialMenuController : MonoBehaviour
{
    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("Referneces")]
    [SerializeField] private RadialMenuView          _view;
    [SerializeField] private RadialMenuAnimator      _animator;
    [SerializeField] private MenuStateManager        _menuStateManager;

    private RadialMenuModel _model;

    // Temporary storage
    private GameObject currentSelected;
    private InventorySlotUI selectedSlotUI;

    private void Awake()
    {
        _model = GetComponent<RadialMenuModel>();

        _view.Initialize(_model);
        _animator.Initialize(_model, _view);

    }

    private void OnEnable()
    {
        inputChannel.OnMouseMoved += HandleMouseMoved;
        inputChannel.OnRadialMenuOpen += HandleOpenRadialMenu;
        inputChannel.OnRadialMenuConfirm += HandleConfirmRadialMenu;
        inputChannel.OnNavigate += HandleNavigate;
    }

    private void OnDisable()
    {
        if(inputChannel != null)
        {
            inputChannel.OnMouseMoved -= HandleMouseMoved;
            inputChannel.OnRadialMenuOpen -= HandleOpenRadialMenu;
            inputChannel.OnRadialMenuConfirm -= HandleConfirmRadialMenu;
            inputChannel.OnNavigate -= HandleNavigate;
        }
    }

    /// <summary>
    /// Handles navigation input to select items
    /// </summary>
    private void HandleNavigate(InputAction.CallbackContext context)
    {
        if (!_model.IsOpen) return;

        Vector2 direction = context.ReadValue<Vector2>();
        int itemCount = _model.currentItems.Count;
        if (itemCount == 0) return;

        int change = 0;
        if(direction.x > 0.5f)
        {
            change = 1; // backward
        }else if(direction.x < -0.5f)
        {
            change = -1; // forward
        }
        if(change != 0)
        {
            int currentIndex = _model.CurrentHighlightIndex;
            if(currentIndex == -1)
            {
                currentIndex = (change > 0) ? -1 : 0;
            }

            // Calculate the new index 
            // If it is a clockwise rotation
            // Circular Queue: nextIndex = (currentIndex + 1) % capacity.
            // When we rotate counterclockwise, [e.g., (0 - 1 + 5) % 5 = 4]
            int newIndex = (currentIndex + change + itemCount) % itemCount;
            _model.CurrentHighlightIndex = newIndex;
        }
    }

    private void HandleOpenRadialMenu()
    {
        if (_model.IsOpen) return;

        currentSelected = _menuStateManager.LastItemSelected;
        if (currentSelected == null) 
            return;

        selectedSlotUI = currentSelected.GetComponent<InventorySlotUI>();
        if (selectedSlotUI == null || selectedSlotUI.slot == null || selectedSlotUI.slot.item == null)
            return;

        Item currentItem = selectedSlotUI.slot.item;

        List<RadialMenuData> menuItems = currentItem.GetRadialMenuItems();

        if (menuItems == null || menuItems.Count == 0)
            return;

        // Ensure that the menu opens at the position of the currently selected item
        _view.transform.position = currentSelected.transform.position;

        _model.OpenMenu(menuItems);
    }

    private void HandleConfirmRadialMenu()
    {
        if (!_model.IsOpen) return;

        Debug.Log("Confirm£¡");

        _model.ConfirmSelection();
        _model.CloseMenu();
    }

    public void HandleMouseMoved(Vector2 mousePosition)
    {
        if (!_model.IsOpen) return;

        // Convert the menu's world position to a screen position to serve as the pivot point.
        Vector2 centerScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, _view.radialPartTransform.position);

        // Calculate the vector from the menu center to the mouse position.
        Vector2 mouseVector = mousePosition - centerScreenPosition;

        if (mouseVector == Vector2.zero)
        {
            _model.CurrentHighlightIndex = -1;
            return;
        }

        // Calculate the angle of the mouse vector. Vector2.up is considered 0 degrees.
        float angle = Vector2.SignedAngle(mouseVector, Vector2.up);

        // Convert the angle from (-180, 180) to (0, 360) range.
        if (angle < 0)
        {
            angle += 360;
        }

        // Determine which slice the angle falls into.
        float angleStep = 360f / _model.currentItems.Count;
        _model.CurrentHighlightIndex = Mathf.FloorToInt(angle / angleStep);
    }
}
