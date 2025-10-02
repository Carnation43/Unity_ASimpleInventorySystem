using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    private Item equippedItem;
    public Image icon;
    public EquipmentSlotType equipmentSlotType;

    /// <summary>
    /// Update slots when equipped or unequipped
    /// </summary>
    /// <param name="item"></param>
    public void UpdateSlot (Item item)
    {
        equippedItem = item;

        if (equippedItem)
        {
            icon.enabled = true;
            icon.sprite = item.sprite;
        }
        else
        {
            icon.enabled = false;
            icon.sprite = null;
        }
    }


}
