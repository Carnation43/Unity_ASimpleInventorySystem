using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeRedDotHandler : MonoBehaviour
{
    [SerializeField] private RecipeEventChannel recipeChannel;
    [SerializeField] private RecipeBookManager recipeManager;

    private void Start()
    {
        if (recipeManager != null)
        {
            foreach (var status in recipeManager.AcquiredRecipes)
            {
                if (status.isNew)
                {
                    SetRedDot(status.recipe, 1);
                }
            }
        }
    }

    private void OnEnable()
    {
        if (recipeChannel != null)
        {
            recipeChannel.OnRecipeAcquired += HandleAcquired;
            recipeChannel.OnRecipeViewed += HandleViewed;
        }
    }

    private void OnDisable()
    {
        if(recipeChannel != null)
        {
            recipeChannel.OnRecipeAcquired -= HandleAcquired;
            recipeChannel.OnRecipeViewed -= HandleViewed;
        }
    }

    private void HandleAcquired(Recipe recipe)
    {
        SetRedDot(recipe, 1);
    }

    private void HandleViewed(Recipe recipe)
    {
        SetRedDot(recipe, 0);
    }

    private void SetRedDot(Recipe recipe, int count)
    {
        if (recipe == null || RedDotManager.Instance == null) return;

        string path = $"{RedDotPaths.Recipe}/{recipe.recipeName}";
        RedDotManager.Instance.GetRedDotNode(path).SetCount(count);
    }

    
}
