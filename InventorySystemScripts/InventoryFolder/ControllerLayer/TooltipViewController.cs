using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Its primary responsibility is to handle all the logic. user input, and data flow related to the tooltip.
/// </summary>
public class TooltipViewController : MonoBehaviour
{
    public static TooltipViewController instance;

    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;
    [SerializeField] private RadialMenuModel radialMenuModel;

    [Header("Dependencies")]
    [SerializeField] private TooltipView tooltipView;
    [SerializeField] private TooltipPosition tooltipPosition;
    [SerializeField] private TooltipAnimator tooltipAnimator;

    // Stores the data of the inventory slot currently being displayed in the tooltip.
    private InventorySlot _currentSlot;

    private GameObject _lastSelectedObject;
    private float _lastActionTime;
    private const float ACTION_COOLDOWN = 0.2f;

    public bool IsHidden => tooltipAnimator == null || tooltipAnimator.IsHidden;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != null) Destroy(gameObject);
        tooltipAnimator.Initialize(tooltipView.detailsButton, tooltipView.equipButton);
    }

    private void OnEnable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnHide += HandleHide;
            inputChannel.OnConfirm += HandleConfirm;
            inputChannel.OnShowDetails += HandleShowDetails;
        }

        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.OnInventoryUpdated += HandleInventoryUpdate;
        }
        if (radialMenuModel != null)
        {
            radialMenuModel.OnMenuStateChanged += HandleRadialMenuStateChanged;
        }
    }

    private void OnDisable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnHide -= HandleHide;
            inputChannel.OnConfirm -= HandleConfirm;
            inputChannel.OnShowDetails -= HandleShowDetails;
        }
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.OnInventoryUpdated -= HandleInventoryUpdate;
        }
        if (radialMenuModel != null)
        {
            radialMenuModel.OnMenuStateChanged -= HandleRadialMenuStateChanged;
        }
    }

    /// <summary>
    /// Let the tooltip actively manage its own state
    /// </summary>
    private void Update()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // if no slot is selected
        if (currentSelected == null)
        {
            // Prevent the tooltip from closing accidentally when the mouse moves over it
            if (!TooltipRaycastTarget.IsPointerOver)
            {
                HideTooltip();
            }
        }
        
        // record currently selected slot
        _lastSelectedObject = currentSelected;
    }

    private void HandleShowDetails(InputAction.CallbackContext context)
    {
        if (tooltipAnimator != null)
        {
            tooltipAnimator.HandleDetailsHoldAnimation(context);
        }
    }

    private void HandleRadialMenuStateChanged(bool isOpen)
    {
        if (isOpen)
        {
            tooltipAnimator.Hide();
        }
        else
        {
            if (_currentSlot != null && tooltipPosition._trackedRectTransform != null)
            {
                ShowTooltip(_currentSlot, tooltipPosition._trackedRectTransform);
            }
        }
    }

    private void HandleHide(InputAction.CallbackContext obj)
    {
        tooltipAnimator.ToggleTooltip(tooltipPosition._trackedRectTransform);
    }

    private void HandleConfirm(InputAction.CallbackContext obj)
    {
        if (_currentSlot == null || _currentSlot.item == null) return;
        if (Time.time - _lastActionTime < ACTION_COOLDOWN) return;

        if (_currentSlot.item.isEquippable)
        {
            PerformEquipAction();
        }
        else if (_currentSlot.item.category == ItemCategory.Consumable)
        {
            PerformConsumeAction();
        }
        else if (_currentSlot.item.category == ItemCategory.Material)
        {
            // TODO: 
        }
    }

    private void PerformConsumeAction()
    {
        _lastActionTime = Time.time;

        tooltipAnimator.TriggerConfirmAnimation();

        CharacterStatsController.instance.RestoreHealth(_currentSlot.item.hp);
        InventoryManager.instance.RemoveItem(_currentSlot);
    }
    private void PerformEquipAction()
    {
        _lastActionTime = Time.time;

        tooltipAnimator.TriggerConfirmAnimation();

        if (_currentSlot.isEquipped)
        {
            EquipmentManager.instance.UnEquip(_currentSlot.item.equipmentSlotType);
        }
        else
        {
            EquipmentManager.instance.Equip(_currentSlot);
        }    
    }

    private void HandleInventoryUpdate()
    {
        if (_currentSlot != null)
        {
            tooltipView.SetTooltip(_currentSlot);
        }
    }

    public void ShowTooltip(InventorySlot slot, RectTransform trackedTransform)
    {
        if (slot == null || slot.item == null)
        {
            HideTooltip();
            return;
        }

        _currentSlot = slot;
        tooltipPosition._trackedRectTransform = trackedTransform;
        tooltipView.SetTooltip(slot);
        tooltipAnimator.Show(trackedTransform);
    }

    public void HideTooltip()
    {
        _currentSlot = null;
        tooltipAnimator.Hide();
    }
}