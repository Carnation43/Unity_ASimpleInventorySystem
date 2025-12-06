using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Handles the user input and logic for the radial menu.
/// </summary>
public class RadialMenuController : MonoBehaviour
{
    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;
    [SerializeField] private CharacterPanelVfxChannel characterVfxChannel;

    [Header("SFX Broadcasting On")]
    [SerializeField] private AudioCueEventChannel uiAudioChannel;
    [SerializeField] private ItemActionEventChannel itemActionChannel;

    [Header("Audio Cues - Self")]
    [SerializeField] private AudioCueSO onNavigateCue;
    [SerializeField] private AudioCueSO onHoldStartCue;
    [SerializeField] private AudioCueSO onMenuOpenCue;

    [Header("Referneces")]
    [SerializeField] private RadialMenuView          _view;
    [SerializeField] private RadialMenuAnimator      _animator;
    [SerializeField] private SelectionStateManager   _menuStateManager;
    [SerializeField] private DetailsContent          _detailsContent;

    [Header("Load Icon")]
    [SerializeField] private Image loadImage;
    [SerializeField] private float loadTime = 0.3f;

    private RadialMenuModel _model;
    private Tweener _loadTweener;

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
        inputChannel.OnRadialMenuHoldStart += HandleHoldStart;
    }

    private void OnDisable()
    {
        if(inputChannel != null)
        {
            inputChannel.OnMouseMoved -= HandleMouseMoved;
            inputChannel.OnRadialMenuOpen -= HandleOpenRadialMenu;
            inputChannel.OnRadialMenuConfirm -= HandleConfirmRadialMenu;
            inputChannel.OnNavigate -= HandleNavigate;
            inputChannel.OnRadialMenuHoldStart -= HandleHoldStart;
        }
    }

    private void HandleHoldStart()
    {
        if (InventoryController.instance != null && InventoryController.instance.currentFocus != InventoryController.MenuFocus.Inventory)
        {
            return;
        }

        if (_model.IsOpen) return;

        currentSelected = _menuStateManager.LastItemSelected;
        if (currentSelected == null) return;

        uiAudioChannel.RaiseEvent(onHoldStartCue);

        loadImage.gameObject.SetActive(true);
        loadImage.fillAmount = 0;

        _loadTweener?.Kill();

        _loadTweener = loadImage.DOFillAmount(1, loadTime).SetEase(Ease.Linear);
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
            uiAudioChannel.RaiseEvent(onNavigateCue);
        }
    }

    private void HandleOpenRadialMenu()
    {
        if (InventoryController.instance != null && InventoryController.instance.currentFocus != InventoryController.MenuFocus.Inventory)
        {
            return;
        }

        _loadTweener?.Kill();

        if(loadImage != null)
        {
            loadImage.gameObject.SetActive(false);
        }

        if (_model.IsOpen) return;

        currentSelected = _menuStateManager.LastItemSelected;
        if (currentSelected == null) 
            return;

        selectedSlotUI = currentSelected.GetComponent<InventorySlotUI>();
        if (selectedSlotUI == null || selectedSlotUI.IData == null || selectedSlotUI.IData.item == null)
            return;

        List<RadialMenuData> menuItems = BuildMenuItemsForSlot(selectedSlotUI.IData);

        if (menuItems == null || menuItems.Count == 0)
            return;

        uiAudioChannel.RaiseEvent(onMenuOpenCue);

        // Ensure that the menu opens at the position of the currently selected item
        _view.transform.position = currentSelected.transform.position;

        _model.OpenMenu(menuItems);
    }

    /// <summary>
    /// Prepare the content of the menu items based on the currently selected item
    /// </summary>
    /// <param name="inventorySlot">Currently selected slot</param>
    /// <returns>the radial menu contains icons and actions</returns>
    private List<RadialMenuData> BuildMenuItemsForSlot(InventorySlot currentSlot)
    {
        Item currentItem = currentSlot.item;
        var actionRequests = currentItem.GetActionRequests();
        var menuItems = new List<RadialMenuData>();

        foreach (var request in actionRequests)
        {
            Action actionToPerform = null;
            bool shouldAdd = true;
            string actionName = request.actionType.ToString();

            if (request.actionType == RadialMenuActionType.Craft)
            {
                actionName = "Craft";
                actionToPerform = () =>
                {
                    InventoryController.instance.OpenCraftMenu();
                };
                menuItems.Add(new RadialMenuData(request.icon, actionName, actionToPerform));
                continue;
            }

            switch (request.actionType)
            {
                case RadialMenuActionType.Equip:
                    if (currentSlot.isEquipped) { shouldAdd = false; }
                    else { actionName = "Equip"; }
                    break;
                case RadialMenuActionType.UnEquip:
                    if (!currentSlot.isEquipped) { shouldAdd = false; }
                    else { actionName = "UnEquip"; }
                    break;
                case RadialMenuActionType.Use: actionName = "Consume"; break;
                case RadialMenuActionType.ShowDetails: actionName = "Details"; break;
                case RadialMenuActionType.Drop: actionName = "Drop"; break;
                case RadialMenuActionType.Sort: actionName = "Sort"; break;
                case RadialMenuActionType.Fix: actionName = "Fix"; break;
                case RadialMenuActionType.Enhance: actionName = "Enhance"; break;
                case RadialMenuActionType.Combine: actionName = "Combine"; break;
                // case RadialMenuActionType.Craft: actionName = "Craft"; break;
                case RadialMenuActionType.Present: actionName = "Present"; break;
                // Other
                default:
                    actionToPerform = () => Debug.Log("--------- No requests ---------");
                    break;
            }
            if (shouldAdd)
            {
                RadialMenuActionType currentActionType = request.actionType;
                actionToPerform = () => itemActionChannel.RaiseEvent(currentSlot, currentActionType);
                menuItems.Add(new RadialMenuData(request.icon, actionName, actionToPerform));
            }
        }
        return menuItems;

    }

    private void HandleConfirmRadialMenu()
    {
        _loadTweener?.Kill();
        if (loadImage != null)
        {
            loadImage.gameObject.SetActive(false);
        }

        if (!_model.IsOpen) return;

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

        // If the mouse is in the deadzone, then cancel the highlight.
        if(mouseVector.magnitude < _view.CenterDeadZoneRadius || mouseVector.magnitude > _view.ActionZoneRadius)
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
