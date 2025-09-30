using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot
{
    public Item item;
    public int count;

    public InventorySlot(Item item)
    {
        this.item = item;
        count = 1;
    }
}
