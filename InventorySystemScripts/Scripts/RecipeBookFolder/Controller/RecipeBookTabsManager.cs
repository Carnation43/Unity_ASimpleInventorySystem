using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inherits from the generic BaseTabsManager to reuse navigation and selecton logic
/// Handles RecipeFilterCategory filtering
/// </summary>
public class RecipeBookTabsManager : BaseTabsManager<RecipeTab, RecipeFilterCategory>
{
    protected override void InitializeTabs()
    {
        tabs = GetComponentsInChildren<RecipeTab>();

        foreach (var tab in tabs)
        {
            tab.Initialize(this);
        }
    }

    public override void SelectTab(RecipeTab selectedTab, bool animate = true, bool triggerEvent = true)
    {

        // ---------------- record move direction -------------------
        int newIndex = -1;
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == selectedTab)
            {
                newIndex = i;
                break;
            }
        }
        if (newIndex != currentTabIndex)
        {
            navigationDirection = newIndex > currentTabIndex ? 1 : -1;
        }
        else
        {
            navigationDirection = 0;
        }
        // ----------------------------------------------------------

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == selectedTab)
            {
                currentTabIndex = i;
                tabs[i].OnSelect();
            }
            else
            {
                tabs[i].OnDeselect();
            }
        }

        if (triggerEvent)
            onTabSelected?.Invoke(); 
    }

    public override void ChangeSubheadingText(RecipeTab selectedTab)
    {
        // Do not exec
    }
}
