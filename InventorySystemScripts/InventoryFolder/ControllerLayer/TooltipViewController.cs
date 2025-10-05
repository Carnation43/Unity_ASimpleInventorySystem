using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            // inputChannel.OnShowDetails += HandleShowDetails;
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
            // inputChannel.OnShowDetails -= HandleShowDetails;
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
        HandleEquipAction();
    }

    public void HandleEquipAction()
    {
        if (_currentSlot == null || _currentSlot.item == null)
            return;

        tooltipAnimator.TriggerConfirmAnimation();

        if (_currentSlot.item.isEquippable)
        {
            if (_currentSlot.isEquipped)
            {
                EquipmentManager.instance.UnEquip(_currentSlot.item.equipmentSlotType);
            }
            else
            {
                EquipmentManager.instance.Equip(_currentSlot);
            }
        }
        else
        {
            // TODO: Other processing logic
        }
    }

    private void HandleShowDetails(InputAction.CallbackContext obj)
    {
        tooltipAnimator.TriggerDetailsAnimation();
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