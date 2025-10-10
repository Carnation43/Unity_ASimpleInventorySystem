using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// It holds the list of all item slots and provides methods to modify it.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    // The master list of all inventory slots. This is the single source of truth for what the player owns.
    public List<InventorySlot> inventory = new List<InventorySlot>();

    // Events are used to decouple the data layer from the UI layer.
    // The InventoryManager doesn't know or care who is listening: it just broadcast that something happened.
    public delegate void InventoryChanged();
    public event InventoryChanged OnItemAdded;
    public event InventoryChanged OnItemRemoved;
    public event Action<InventorySlot, bool, int> OnItemConsumed; // bool to indicate if the slot is now empty

    public event Action OnInventoryUpdated;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// A public method allowing other systems (like EquipmentManager) to request a UI refresh
    /// without modifying the inventory list itself.
    /// </summary>
    public void TriggerInventoryUpdate()
    {
        OnInventoryUpdated?.Invoke();
    }

    #region [Deprecated method: FindSlotByItem]
    /// <summary>
    /// A helper method to find a specific inventory slot based on the item data it contains.
    /// This is used by EquipmentManager to mark items as equipped/unequipped.
    /// </summary>
    /// <param name="itemToFind">The item data to search for</param>
    /// <returns>The found InventorySlot, or null if not found.</returns>
    public InventorySlot FindSlotByItem(Item itemToFind)
    {
        return inventory.Find(slot => slot.item == itemToFind);
    }
    #endregion

    public void AddItem(Item newItem)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            // New items that can be stacked, then the quantity increments by 1.
            if (inventory[i].item == newItem && inventory[i].item.stackable)
            {
                inventory[i].count++;
                OnInventoryUpdated?.Invoke();
                return;
            }
        }
        inventory.Add(new InventorySlot(newItem));
        OnItemAdded?.Invoke();      // trigger event
    }

    public void RemoveItem(InventorySlot slot)
    {
        if (slot == null || !inventory.Contains(slot)) return;
        int slotIndex = inventory.IndexOf(slot);
        slot.count--;
        bool slotIsEmpty = slot.count <= 0;

        OnItemConsumed?.Invoke(slot, slotIsEmpty, slotIndex);

        if (slotIsEmpty)
        { 
            inventory.Remove(slot);
            OnItemRemoved?.Invoke();
        }
        else
        {
            OnInventoryUpdated?.Invoke();
        }
    }      
}
