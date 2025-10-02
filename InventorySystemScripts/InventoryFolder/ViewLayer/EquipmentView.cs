using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the UI panel that displays the character's currently equipped items
/// It listens for changes from the EquipmentManager and updates the corresponding EquipmentSlotUI components.
/// </summary>
public class EquipmentView : MonoBehaviour
{
    // The parent transform that holds all the individual equipment slot UI elements.
    public Transform equipmentSlotsParent;
    public Dictionary<EquipmentSlotType, EquipmentSlotUI> equipmentSlots = new Dictionary<EquipmentSlotType, EquipmentSlotUI>();

    private void OnEnable()
    {
        if (EquipmentManager.instance != null)
        {
            EquipmentManager.instance.OnEquipmentChanged += RefreshEquipmentUI;
        }
    }

    private void OnDisable()
    {
        if(EquipmentManager.instance != null)
        {
            EquipmentManager.instance.OnEquipmentChanged -= RefreshEquipmentUI;
        }   
    }

    private void Start()
    {
        Initialize();
    }

    // Initialize all equipment slots
    private void Initialize()
    {
        // Find all the slots under the equipment bar, establish a mapping relationship, and record them in a dictionary.
        foreach (EquipmentSlotUI slot in equipmentSlotsParent.GetComponentsInChildren<EquipmentSlotUI>())
        {
            equipmentSlots[slot.equipmentSlotType] = slot;
        }
        RefreshEquipmentUI();
    }

    // Refresh the equipment UI panel
    private void RefreshEquipmentUI()
    {
        foreach (var slot in equipmentSlots)
        {
            // The Key is the EquipmentSlotType
            // The Value is a game object with EquipmentSlotUI.
            if (EquipmentManager.instance.equippedItems.TryGetValue(slot.Key, out Item item))
            {
                slot.Value.UpdateSlot(item);
            }
            else
            {
                slot.Value.UpdateSlot(null);
            }
        }
    }
}
