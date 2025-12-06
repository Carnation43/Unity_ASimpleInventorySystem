using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using InstanceResetToDefault;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Manages the behaivor of the tab group in the inventory menu.
/// It handles the selection, navigation via input, and visual feedback like animations and cooldowns.
/// </summary>
public class TabsManager : BaseTabsManager<Tab, ItemCategory>
{
    [Header("Input Listening To")]
    [SerializeField] private InputEventChannel inputChannel;

    protected override void InitializeTabs()
    {
        tabs = GetComponentsInChildren<Tab>();

        foreach (var tab in tabs)
        {
            tab.Initialize(this);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (inputChannel != null)
        {
            inputChannel.OnNavigateLeft += HandleNavigateLeft;
            inputChannel.OnNavigateRight += HandleNavigateRight;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (inputChannel != null)
        {
            inputChannel.OnNavigateLeft -= HandleNavigateLeft;
            inputChannel.OnNavigateRight -= HandleNavigateRight;
        }
    }

    private void HandleNavigateLeft(InputAction.CallbackContext context)
    {
        if (InventoryController.instance == null || InventoryController.instance.currentFocus != InventoryController.MenuFocus.Inventory)
        {
            return;
        }

        if ((inputChannel != null && inputChannel.IsInputLocked) || UserInput.IsRadialMenuHeldDown) return;

        NavigateTabs(-1);
    }

    private void HandleNavigateRight(InputAction.CallbackContext context)
    {
        if (InventoryController.instance == null || InventoryController.instance.currentFocus != InventoryController.MenuFocus.Inventory)
        {
            return;
        }

        if ((inputChannel != null && inputChannel.IsInputLocked) || UserInput.IsRadialMenuHeldDown) return;

        NavigateTabs(1);
    }
}