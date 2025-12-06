using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a specific tab in the Inventory Menu
/// Inherits from BaseTab with ItemCategory
/// </summary>
public class Tab : BaseTab<ItemCategory>
{
    // Temp
    public TabsManager tabsManager;

    public void Initialize(TabsManager tabsManager)
    {
        this.tabsManager = tabsManager;
    }

    public override void OnClick()
    {
        if (tabsManager != null)
        {
            tabsManager.SelectTab(this);
        }
    }
}
