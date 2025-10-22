using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define the number of needed materials
/// </summary>
[Serializable]
public class ItemQuantity
{
    public Item item;

    [Tooltip("Needed items")]
    [Range(1, 99)]
    public int count = 1;
}
