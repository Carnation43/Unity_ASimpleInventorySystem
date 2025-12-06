using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager for the Recipe Book System.
/// Handles the list of acquired recipes, their unlock status, and filtering logic.
/// </summary>
public class RecipeBookManager : MonoBehaviour
{
    public static RecipeBookManager instance;

    [Header("Data Source")]
    [Tooltip("Reference from CraftingManager")]
    [SerializeField] private CraftingManager _craftingManager;
    [SerializeField] private PlayerWallet_SO _playerWallet;

    public List<RecipeStatus> AcquiredRecipes { get; private set; } = new List<RecipeStatus>();

    // TODO:
    // public event Action OnRecipeAcquired;
    // public event Action OnRecipeUnlocked;

    // --- DEBUG ---
    public event Action OnRecipeDataChanged;
    // --- DEBUG ---

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        // --- Debug --- //
        // Auto-populate the recipe book with all available recipes from the CraftingManager.
        if (_craftingManager != null && _craftingManager.AllRecipes != null)
        {
            foreach (Recipe recipe in _craftingManager.AllRecipes)
            {
                AcquiredRecipes.Add(new RecipeStatus(recipe));
            }
            Debug.Log($"[RecipeBookManager] Debug, automatically acquired {AcquiredRecipes.Count} recipes");
        }
        else
        {
            Debug.LogError("[RecipeBookManager] _craftingManager not configured");
        }

        // --- Debug --- //
    }

    // TODO:
    public void AcquireRecipe(Recipe recipe)
    {

    }

    // TODO:
    public bool UnlockRecipe(RecipeStatus recipeStatus)
    {
        if (recipeStatus == null || recipeStatus.isUnlocked)
        {
            return false; 
        }

        if (_playerWallet == null)
        {
            Debug.LogError("[RecipeBookManager] unbound wallet !");
            return false;
        }

        int cost = recipeStatus.recipe.inspirationCost;

        // Try to deduct the cost
        if (_playerWallet.TrySpendInspiration(cost))
        {
            recipeStatus.isUnlocked = true;

            // TODO: Trigger OnRecipeUnlock event here for UI effects
            return true;
        }
        return false;
    }

    // TODO: Filter page
    public List<RecipeStatus> GetFilteredRecipes(RecipeFilterCategory category)
    {
        switch (category)
        {
            case RecipeFilterCategory.All:
                return AcquiredRecipes;

            case RecipeFilterCategory.Locked:
                return AcquiredRecipes.Where(rs => !rs.isUnlocked).ToList();

            case RecipeFilterCategory.Potion:
                return AcquiredRecipes.Where(rs => rs.recipe.FilterCategory == RecipeFilterCategory.Potion).ToList();

            case RecipeFilterCategory.Melee:
                return AcquiredRecipes.Where(rs => rs.recipe.FilterCategory == RecipeFilterCategory.Melee).ToList();

            case RecipeFilterCategory.Ranged:
                return AcquiredRecipes.Where(rs => rs.recipe.FilterCategory == RecipeFilterCategory.Ranged).ToList();

            case RecipeFilterCategory.Special:
                return AcquiredRecipes.Where(rs => rs.recipe.FilterCategory == RecipeFilterCategory.Special).ToList();

            default:
                return new List<RecipeStatus>(); // return null by default

        }
    }

    // Helper to check how many craftable recipes based on current inventory
    public int CalculateMaxCraftableAmount(Recipe recipe)
    {
        if (recipe == null) return 0;
        int maxAmount = int.MaxValue;

        foreach (var ingredient in recipe.ingredients)
        {
            if (ingredient.item == null || ingredient.count <= 0) return 0;

            int storageCount = InventoryManager.instance.GetItemCount(ingredient.item);
            int possibleCrafts = storageCount / ingredient.count;

            if (possibleCrafts < maxAmount)
            {
                maxAmount = possibleCrafts;
            }

            if (maxAmount == 0) return 0;
        }
        return maxAmount == int.MaxValue ? 0 : maxAmount;
    }
}


