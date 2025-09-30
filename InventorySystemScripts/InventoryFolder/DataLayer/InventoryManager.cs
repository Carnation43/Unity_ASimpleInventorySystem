using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public List<InventorySlot> inventory = new List<InventorySlot>();

    // dynamic add or remove card slots
    public delegate void InventoryChanged();
    public event InventoryChanged OnItemAdded;
    public event InventoryChanged OnItemRemoved;

    private void Awake()
    {
        instance = this;
    }

    public void AddItem(Item newItem)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            // New items that can be stacked, then the quantity increments by 1.
            if (inventory[i].item == newItem && inventory[i].item.stackable)
            {
                inventory[i].count++;
                return;
            }
        }
        inventory.Add(new InventorySlot(newItem));
        OnItemAdded?.Invoke();      // trigger event
    }

    public void RemoveItem(Item oldItem)
    {
        inventory.Remove(new InventorySlot(oldItem));
        OnItemRemoved?.Invoke();
    }      
}
