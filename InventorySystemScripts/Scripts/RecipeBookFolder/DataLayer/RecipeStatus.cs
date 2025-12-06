using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Package the Recipe ScriptableObject 
/// and attach player-specific states 
/// (whether it is unlocked, whether it is new)
/// </summary>
[Serializable]
public class RecipeStatus
{
    public Recipe recipe;
    public bool isUnlocked;
    public bool isNew;

    public RecipeStatus(Recipe _recipe)
    {
        this.recipe = _recipe;
        this.isUnlocked = false;
        this.isNew = true;
    }
}
