using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Recipe Event Channel")]
public class RecipeEventChannel : ScriptableObject
{
    /// <summary>
    /// Event triggered when acquires a new recipe
    /// Used for HUD notification, Red Dot System, Auido, Effect...
    /// </summary>
    public event Action<Recipe> OnRecipeAcquired;

    /// <summary>
    /// Event triggered when the recipe content changes
    /// Listeners should refresh their grid when this happens
    /// </summary>
    public event Action OnRecipeDataChanged;

    /// <summary>
    /// Event triggered when the recipe is viewed
    /// Listners should turn off the red dot
    /// </summary>
    public event Action<Recipe> OnRecipeViewed;

    public void RaiseRecipeAcquired(Recipe recipe)
    {
        if (recipe == null) return;
        OnRecipeAcquired?.Invoke(recipe);
    }

    public void RaiseRecipeDataChanged()
    {
        OnRecipeDataChanged?.Invoke();
    }

    public void RaiseRecipeViewed(Recipe recipe)
    {
        OnRecipeViewed?.Invoke(recipe);
    }
}
