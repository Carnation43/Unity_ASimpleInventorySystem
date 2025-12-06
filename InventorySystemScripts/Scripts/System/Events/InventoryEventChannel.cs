using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This SO acts as a bridge for inventory-related events.
/// It decouples the InventoryManager from the InventoryController/View
/// </summary>
[CreateAssetMenu(menuName = "Events/Inventory Event Channel")]
public class InventoryEventChannel : ScriptableObject
{
    /// <summary>
    /// Event triggered when the inventory content changes (Item Added, removed, or count changed)
    /// Listeners should refresh their grid when this happens.
    /// </summary>
    public event Action OnInventoryUpdated;

    /// <summary>
    /// Event triggered specifically when an item is consumed.
    /// </summary>
    /// <param name="slot"> The inventory slots that was consumed. </param>
    /// <param name="isDepleted"> True if the item stack is now empty. </param>
    /// <param name="slotIndex"> The original index of the slot in the list. </param>
    public event Action<InventorySlot, bool, int> OnItemConsumed;

    /// <summary>
    /// Broadcasts that the inventory has been updated.
    /// Called by InventoryManager
    /// </summary>
    public void RaiseInventoryUpdated()
    {
        if (OnInventoryUpdated != null)
        {
            OnInventoryUpdated?.Invoke();
        }
        else
        {
            Debug.LogWarning("Inventory Updated event raised, but no one is listening.");
        }
    }

    /// <summary>
    /// Broadcasts that an item has been consumed.
    /// Called by InventoryManager
    /// </summary>
    public void RaiseItemConsumed(InventorySlot slot, bool isDepleted, int slotIndex)
    {
        OnItemConsumed?.Invoke(slot, isDepleted, slotIndex);
    }

    // TODO: RaiseItemAdded                        --> HUD Notification
    // TODO: RaiseKeyItemOotained                  --> KEY Tooltip
    // TODO: RaiseItemDrop                         --> DROP | SELL | DELIVERY
    // TODO: RaiseInventoryFull                    --> UI Notification
    // TODO: RaiseItemEquipped                     --> EQUIP | UNEQUIP
}
