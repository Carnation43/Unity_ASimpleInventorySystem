using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manager the player's equipment. This includes equipping and unequipping items.
/// It maintanis a dictonary of currently equipped items for fast access
/// and modifies the 'isEquipped' state of InventorySlots
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;

    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("References")]
    [SerializeField] private SelectionStateManager stateManager; // Gets currently selected item

    // deprecated
    // [SerializeField] private AudioManager audioManager;
    // [SerializeField] private AudioSheetSO equipSfxSheet;
    // [SerializeField] private float equipPitch = 1.15f;
    // [SerializeField] private float unequipPitch = 0.85f;

    // A dictonary that stores the currently equipped item for each equipment slot type.

    public Dictionary<EquipmentSlotType, Item> equippedItems = new Dictionary<EquipmentSlotType,Item>();

    public event Action OnEquipmentChanged;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != null) Destroy(instance);
    }

    public void Equip(InventorySlot slotToEquip)
    {
        if (slotToEquip == null || !slotToEquip.item.isEquippable || slotToEquip.item == null) return;

        Item itemToEquip = slotToEquip.item;

        // Check if there is already an equipment of this type in the equipment slot.
        if (equippedItems.ContainsKey(itemToEquip.equipmentSlotType))
            UnEquip(itemToEquip.equipmentSlotType);

        // Determine the equipment type of itemToEquip and place it in the corresponding key.
        equippedItems[itemToEquip.equipmentSlotType] = itemToEquip;
        slotToEquip.isEquipped = true;

        // Execute RefreshEquipmentUI in EquipmentView.cs
        OnEquipmentChanged?.Invoke();

        InventoryManager.instance.TriggerInventoryUpdate();
    }

    public void UnEquip(EquipmentSlotType slotType)
    {
        // Try to get the item in the equipment slot
        if (equippedItems.TryGetValue(slotType, out Item itemToUnEquip))
        {
            // Find the first item that meets the criteria
            // 1. slot is not null
            // 2. Determine whether the item in the equipment slot is an item from the inventory.
            // 3. marked as equipped
            InventorySlot slot = InventoryManager.instance.inventory.FirstOrDefault(s => s != null && s.item == itemToUnEquip && s.isEquipped);
            if (slot != null)
            {
                slot.isEquipped = false;
            }

            equippedItems.Remove(slotType);

            OnEquipmentChanged?.Invoke(); 
            InventoryManager.instance.TriggerInventoryUpdate();
        }
    }

    public void UpdateCharacterStats()
    {
        Debug.Log("Update Character...");
    }
}
