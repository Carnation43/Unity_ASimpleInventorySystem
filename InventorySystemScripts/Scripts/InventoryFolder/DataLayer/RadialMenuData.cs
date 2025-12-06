using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the data for a single item in the radial menu.
/// It holds the visual icon and the action to be executed upon confirmation.
/// </summary>
public class RadialMenuData
{
    public Sprite icon;
    public Action OnConfirmAction;
    public string actionName;

    /// <summary>
    /// Constructor for creating a new radial menu data entry.
    /// </summary>
    /// <param name="_icon">the icon to display</param>
    /// <param name="_onConfirmAction">the action to execute when this item is confirmed</param>
    public RadialMenuData(Sprite _icon, string _actionName, Action _onConfirmAction)
    {
        this.icon = _icon;
        this.OnConfirmAction = _onConfirmAction;
        this.actionName = _actionName;
    }
}
    
