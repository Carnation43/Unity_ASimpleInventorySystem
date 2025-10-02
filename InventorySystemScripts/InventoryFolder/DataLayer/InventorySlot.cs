using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot
{
    public Item item;
    public int count;
    public bool isEquipped;

    public InventorySlot(Item item)
    {
        this.item = item;
        this.count = 1;
        this.isEquipped = false;
    }
}
