using InstanceResetToDefault;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages all the backend logic for the crafting system.
/// </summary>
public class CraftingManager : MonoBehaviour, IResettableData
{
    public static CraftingManager instance;

    [Header("Recipes Data")]
    [Tooltip("A list of all available Recipe ScriptableObjects in the game.")]
    [SerializeField] private List<Recipe> allRecipes;
    [Tooltip("Recipes Data Dependency")]
    [SerializeField] private RecipeBookManager _recipeBookManager;

    [Header("Inventory Slots Stats")]
    private List<InventorySlot> _craftingSlots = new List<InventorySlot>(3);

    // a dictonary for recipes(Key: combined string, Value: recipe)
    private Dictionary<string, Recipe> _recipeDictionary;

    // a temp slot stores items which are crafted but not claimed
    private InventorySlot _craftedItemSlot;

    // Public property to expose the currently matched recipe
    // It is null if no recipe matches the current ingredients
    public Recipe MatchedRecipe { get; private set; }

    // Public read-onlu access to the current state of the crafting slots.
    public IReadOnlyList<InventorySlot> CraftingSlots => _craftingSlots;
    // completed item slot
    public InventorySlot CraftedItemSlot => _craftedItemSlot;

    public IReadOnlyList<Recipe> AllRecipes => allRecipes;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject);

        // Pre-process all recipes into a dictionary
        InitializeRecipeDictionary();

        _craftingSlots = new List<InventorySlot>();
        for (int i = 0; i < 3; i++)
        {
            _craftingSlots.Add(null); 
        }
    }

    private void OnEnable()
    {
        if (GameResetManager.Instance != null)
        {
            GameResetManager.Instance.Register(this);
        }
    }

    private void OnDisable()
    {
        if (GameResetManager.Instance != null)
        {
            GameResetManager.Instance.UnRegister(this);
        }
    }

    /// <summary>
    /// Iterate through all recipes, generate a unique key for each recipe, 
    /// and store in the dictionary.
    /// </summary>
    private void InitializeRecipeDictionary()
    {
        _recipeDictionary = new Dictionary<string, Recipe>();

        foreach (var recipe in allRecipes)
        {
            // Immediately perform the projection calculation and store the result in the List.
            List<string> ingredientNames = recipe.ingredients
                .Select(ingredient => ingredient.item.GetInstanceID().ToString())
                .ToList();

            // Sort the names alphabetically. This ensures that the order of
            // ingredients doesn't matter (e.g., "Wood_Stone" and "Stone_Wood"
            // will both resolve to the same key).
            ingredientNames.Sort();

            // Join the sorted names into a single string to use as the key.
            string recipeKey = string.Join("_", ingredientNames);

            if (_recipeDictionary.ContainsKey(recipeKey))
            {
                Debug.LogWarning($"Duplicate recipe found! Key: {recipeKey}");
            }
            else
            {
                _recipeDictionary.Add(recipeKey, recipe);
            }
        }
    }

    /// <summary>
    /// Check if the items in the current crafting grid match the known recipes
    /// This method will be called every time the content of the grid changes
    /// </summary>
    private void CheckForMatchingRecipe()
    {
        // Get a list of currently placed ingredients (filtering out null slots)
        var currentIngredients = _craftingSlots.Where(s => s != null).ToList();

        // If there is no item in the slot, no check will be performed.
        if (_craftingSlots.All(slot => slot == null))
        { 
            MatchedRecipe = null;
            return;
        }

        // generate the key of the current item in the slot
        List<string> currentItemNames = currentIngredients
            .Select(slot => slot.item.GetInstanceID().ToString())
            .ToList();
        currentItemNames.Sort();
        string currentKey = string.Join("_", currentItemNames);

        // Look up in the dictionary
        if (_recipeDictionary.TryGetValue(currentKey, out Recipe foundRecipe))
        {
            if (_recipeBookManager != null && !_recipeBookManager.IsRecipeUnlocked(foundRecipe))
            {
                MatchedRecipe = null;
                Debug.Log($"[CraftingManager] Recipe found {foundRecipe.recipeName} but not unlocked yet.");
                return;
            }

            // Check Quantity
            if(DoQuantitiesMatch(foundRecipe, currentIngredients))
            {
                MatchedRecipe = foundRecipe;
                Debug.Log("Successfully found recipe: " + foundRecipe.recipeName);
                return;
            }
        }

        // no matching recipe
        MatchedRecipe = null;
    }

    /// <summary>
    /// Hepler method to verify if the quantities of items in the slots
    /// meet the recipe's requirments.
    /// </summary>
    /// <param name="currentSlots">current items in the crafting grid</param>
    private bool DoQuantitiesMatch(Recipe recipe, List<InventorySlot> currentSlots)
    {
        // Iterate through each ingredient required by the recipe
        foreach (var recipeIngredient in recipe.ingredients)
        {
            // Find the corresponding item in the crafting grid.
            var placedSlot = currentSlots.FirstOrDefault(slot => slot.item == recipeIngredient.item);

            // If the item is not found, or the quantiity is less than required, the match fails.
            if (placedSlot == null || placedSlot.count < recipeIngredient.count)
            {
                return false;
            }
        }
        // ok !
        return true;
    }

    /// <summary>
    /// Try to add item to the specified slot
    /// </summary>
    /// <param name="slotIndex">which slot</param>
    /// <param name="itemSlot">item to be added</param>
    public bool AddItemToSlot(int slotIndex, InventorySlot itemSlot)
    {
        if (slotIndex < 0 || slotIndex >= 3) return false;

        // If the target slot is empty, place the item there.
        if (_craftingSlots[slotIndex] == null)
        {
            _craftingSlots[slotIndex] = itemSlot;
        }
        // If the target slot contains the same item, stack it.
        else if (_craftingSlots[slotIndex].item == itemSlot.item && _craftingSlots[slotIndex].item.stackable)
        {
            _craftingSlots[slotIndex].count += itemSlot.count;
        }
        else // If the slot contains a differrent item, the add operation fails.
        {
            return false;
        }

        // after any change, re-check for a matching recipe
        CheckForMatchingRecipe();
        return true;
    }

    /// <summary>
    /// Removes the entire stack of items from a specified slot.
    /// </summary>
    /// <param name="slotIndex">The index of the slot.</param>
    /// <returns>The InventorySlot that was removed</returns>
    public InventorySlot RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 3 || _craftingSlots[slotIndex] == null) return null;

        InventorySlot removedSlot = _craftingSlots[slotIndex];
        _craftingSlots[slotIndex] = null;
        CheckForMatchingRecipe();
        return removedSlot;
    }

    public void CraftItem()
    {
        CraftItems(1);
    }

    public void CraftAllItems()
    {
        int amountToCraft = CalculateMaxCraftableAmount();
        CraftItems(amountToCraft);
    }

    /// <summary>
    /// Craft one instance of the currently matched recipe
    /// </summary>
    private void CraftItems(int amountToCraft)
    {
        if (MatchedRecipe == null || amountToCraft <= 0) return;

        // 1. Consume ingredients from the crafting slots
        foreach (var ingredient in MatchedRecipe.ingredients)
        {
            var slotToConsume = _craftingSlots.FirstOrDefault(s => s != null && s.item == ingredient.item);
  
            if (slotToConsume != null)
            {
                slotToConsume.count -= ingredient.count * amountToCraft;
            }
        }

        for (int i = 0; i < _craftingSlots.Count; i++)
        {
            if (_craftingSlots[i] != null && _craftingSlots[i].count <= 0)
            {
                _craftingSlots[i] = null;
            }
        }

        var resultItem = MatchedRecipe.result.item;
        int totalItemsToAdd = MatchedRecipe.result.count * amountToCraft;

        if (_craftedItemSlot == null)
        {
            _craftedItemSlot = new InventorySlot(resultItem) { count = totalItemsToAdd };
        }
        else // It won't be executed here.
        {
            _craftedItemSlot.count += totalItemsToAdd;
        }

        // Re-check for a recipe, as consuming ingredients might allow another craft.
        CheckForMatchingRecipe();
    }

    /// <summary>
    /// Removes a single unit of an item from a specified slot.
    /// </summary>
    /// <param name="slotIndex">The index of the slot.</param>
    /// <returns>The Item that was removed, or null if the slot was empty.</returns>
    public Item RemoveSingleItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 3 || _craftingSlots[slotIndex] == null)
        {
            return null;
        }

        var slot = _craftingSlots[slotIndex];
        Item itemToReturn = slot.item;

        // Decrease the count by one.
        slot.count--;

        // Clear the slot
        if (slot.count <= 0)
        {
            _craftingSlots[slotIndex] = null;
        }
        
        CheckForMatchingRecipe();
        return itemToReturn;
    }

    /// <summary>
    /// Calculates the maximum number of times the current recipe can be crafted
    /// with the ingredients currently in the grid.
    /// </summary>
    /// <returns>The maximum craftable amount</returns>
    public int CalculateMaxCraftableAmount()
    {
        if (MatchedRecipe == null) return 0;
        int maxAmount = int.MaxValue;

        foreach (var ingredient in MatchedRecipe.ingredients)
        {
            var slot = _craftingSlots.FirstOrDefault(s => s != null && s.item == ingredient.item);
            if (slot == null || ingredient.count == 0) return 0;

            int possibleCrafts = slot.count / ingredient.count;
            if (possibleCrafts < maxAmount)
            {
                maxAmount = possibleCrafts;
            }
        }
        return maxAmount == int.MaxValue ? 0 : maxAmount;
    }

    /// <summary>
    /// This is the response to the player's action of claiming the crafted item.
    /// </summary>
    public void RetrieveCraftedItems()
    {
        if (_craftedItemSlot == null) return;

        for (int i = 0; i < _craftedItemSlot.count; i++)
        {
            InventoryManager.instance.AddItem(_craftedItemSlot.item);
        }

        _craftedItemSlot = null;
    }

    // Called when the crafting menu closes.
    public void RetrieveAllItemsToInventory()
    {
        if (_craftingSlots != null)
        {
            foreach (var slot in _craftingSlots)
            {
                if (slot != null && slot.item != null)
                {
                    for (int i = 0; i < slot.count; i++)
                    {
                        InventoryManager.instance.AddItem(slot.item);
                    }
                }
            }
        }

        for (int i = 0; i < _craftingSlots.Count; i++) _craftingSlots[i] = null;
        MatchedRecipe = null;
    }

    public void ResetData()
    {

        _craftingSlots = new List<InventorySlot>(3);
        for (int i = 0; i < 3; i++)
        {
            _craftingSlots.Add(null);
        }

        MatchedRecipe = null;

        Debug.Log("[CraftingManager] Data has been WIPED for New Game.");
    }
}
