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

    [Header("Broadcasting On")]
    [SerializeField] private RecipeEventChannel _recipeChannel;

    [Header("Data Source")]
    [Tooltip("Reference from CraftingManager")]
    [SerializeField] private CraftingManager _craftingManager;
    [SerializeField] private PlayerWallet_SO _playerWallet;

    public List<RecipeStatus> AcquiredRecipes { get; private set; } = new List<RecipeStatus>();

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
    }

    public bool CanAfford(int cost)
    {
        if (_playerWallet == null)
        {
            Debug.LogWarning("[RecipeBookManager] Wallet is missing !");
            return false;
        }

        return _playerWallet.CurrentInspiration >= cost;
    }

    // TODO:
    public void AcquireRecipe(Recipe recipe)
    {
        if (recipe == null) return;

        bool alreadyHas = AcquiredRecipes.Exists(status => status.recipe == recipe);
        if (alreadyHas)
        {
            Debug.Log($"[RecipeBookManager_AcquireRecipe] Already has recipe: {recipe.recipeName}");
            return;
        }

        RecipeStatus newStatus = new RecipeStatus(recipe);
        AcquiredRecipes.Add(newStatus);

        if (_recipeChannel != null)
        {
            _recipeChannel.RaiseRecipeAcquired(recipe);

            _recipeChannel.RaiseRecipeDataChanged();
        }
        else
        {
            Debug.LogWarning("[RecipeBookManager_AcquireRecipe] Recipe Event Channel is missing!");
        }

        Debug.Log($"<color=green>[RecipeBookManager_AcquireRecipe] Acquire new recipe: {recipe.recipeName}</color>"); 
    }

    public void MarkRecipeViewed(RecipeStatus status)
    {
        if (status == null || !status.isNew) return;

        status.isNew = false;

        if (_recipeChannel != null)
        {
            _recipeChannel.RaiseRecipeViewed(status.recipe);
        }
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

    public bool IsRecipeUnlocked(Recipe recipe)
    {
        if (recipe == null) return false;

        var status = AcquiredRecipes.Find(s => s.recipe == recipe);

        return status != null && status.isUnlocked;
    }

    // --- Debug START ---
    [ContextMenu("DEBUG: Acquire a recipe")]
    public void Debug_AcquireRecipe()
    {
        if (_craftingManager == null || _craftingManager.AllRecipes == null)
        {
            Debug.LogWarning("[RecipeBookManager] CraftingManager is missing");
        }

        List<Recipe> unacquiredRecipes = _craftingManager.AllRecipes
            .Where(r => !AcquiredRecipes.Exists(status => status.recipe == r))
            .ToList();

        if (unacquiredRecipes.Count == 0)
        {
            Debug.LogWarning("[RecipeBookManager_Debug] You have already acquired all recipes");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, unacquiredRecipes.Count);
        Recipe targetRecipe = unacquiredRecipes[randomIndex];

        AcquireRecipe(targetRecipe);
    }
    // --- Debug END ---
}


