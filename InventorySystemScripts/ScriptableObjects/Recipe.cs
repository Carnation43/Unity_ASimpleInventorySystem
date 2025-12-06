using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "ScriptableObjects/Recipe")]
public class Recipe : ScriptableObject
{
    [Header("Basic Information")]
    public string recipeName;
    [TextArea(5, 10)]
    public string recipeDescription;
    [SerializeField] private RecipeFilterCategory filterCategory;

    [Header("Unlock Settings")]
    public int inspirationCost = 1;

    [Tooltip("Production Materials")]
    public List<ItemQuantity> ingredients;

    public ItemQuantity result;

    // recipe result icon
    public Sprite recipeIcon
    {
        get
        {
            if (result != null && result.item != null)
            {
                return result.item.sprite;
            }
            return null;
        }
    }

    public RecipeFilterCategory FilterCategory => filterCategory;

    private void OnValidate()
    {
        if (ingredients != null && ingredients.Count > 3)
        {
            Debug.LogWarning($"Recipe '{name}' has more than 3 ingreidents. Excess ingredients will be removed");
            // Directly truncate the list to ensure it dosen't break the UI
            ingredients.RemoveRange(3, ingredients.Count - 3);
        }

        // Ensure that all fields are set
        if (result == null || result.item == null)
        {
            Debug.LogWarning($"Recipe '{name}' is missing a Result.");
        }
        if (ingredients == null || ingredients.Count == 0)
        {
            Debug.LogWarning($"Recipe '{name}' has no Ingredients.");
        }
    }
}

// Enum definition for recipe categories
public enum RecipeFilterCategory
{
    All,
    Locked,
    Potion,
    Melee,
    Ranged,
    Special
}
