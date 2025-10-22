using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inventory slot,
/// Radial menu action type
/// </summary>
public struct ItemActionRequest
{
    public InventorySlot Slot;
    public RadialMenuActionType ActionType;
}

/// <summary>
/// Perform an action on this inventory
/// The bridge between UI and ActionManager
/// </summary>
[CreateAssetMenu(menuName = "Events/Item Action Event Channel")]
public class ItemActionEventChannel : ScriptableObject
{
    public event Action<ItemActionRequest> OnActionRequested;

    public void RaiseEvent(InventorySlot slot, RadialMenuActionType actionType)
    {
        var request = new ItemActionRequest { Slot = slot, ActionType = actionType };
        OnActionRequested?.Invoke(request);
    }
}
