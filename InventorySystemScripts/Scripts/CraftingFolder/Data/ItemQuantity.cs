using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define the number of needed materials
/// Simple data class used by Recipes to define ingredients and results
/// </summary>
[Serializable]
public class ItemQuantity
{
    public Item item;

    [Tooltip("Needed items")]
    [Range(1, 99)]
    public int count = 1;
}
