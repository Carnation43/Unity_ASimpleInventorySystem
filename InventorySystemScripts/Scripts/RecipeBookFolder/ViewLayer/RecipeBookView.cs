using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Inherits from the generic BaseGridView to reuse the grid generation and animation logic
/// T_SlotData = RecipeStatus
/// T_SlotUI = RecipeSlotUI
/// </summary>
public class RecipeBookView : BaseGridView<RecipeStatus, RecipeSlotUI>
{
    public static RecipeBookView instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
