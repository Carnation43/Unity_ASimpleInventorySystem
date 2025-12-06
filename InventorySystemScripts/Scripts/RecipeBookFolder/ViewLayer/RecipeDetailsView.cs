using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the panel of the Recipe Book
/// </summary>
public class RecipeDetailsView : MonoBehaviour
{
    [Header("UI References")]

    [Header("Section 1")]
    [SerializeField] private Image headerIcon;
    [SerializeField] private TMP_Text recipeName;
    [SerializeField] private TMP_Text storageText;
    [SerializeField] private TMP_Text craftableText;

    [Header("Section 2")]
    [SerializeField] private TMP_Text recipeDescription;

    [Header("Section 3")]
    [SerializeField] private List<RecipeIngredientSlotUI> ingredientSlots;

    [Header("Root GameObject")]
    [SerializeField] private GameObject detailsRootObject;

    private void Start()
    {
        ClearDetails();
    }

    public void DisplayRecipe(Recipe recipe)
    {
        Debug.Log($"[RecipeDetailsView receive {recipe.recipeName} from controller]");
        if (recipe == null)
        {
            ClearDetails();
            return;
        }
        if (detailsRootObject != null)
        {
            detailsRootObject.SetActive(true); 
        }

        if (headerIcon != null)
        {
            headerIcon.sprite = recipe.recipeIcon;
            headerIcon.enabled = true;
        }

        recipeName.text = recipe.recipeName;
        recipeDescription.text = recipe.recipeDescription;

        if (storageText != null && InventoryManager.instance != null)
        {
            storageText.text = "Storage: " + InventoryManager.instance.GetItemCount(recipe.result.item).ToString();
        }

        if (craftableText != null && RecipeBookManager.instance != null)
        {
            craftableText.text = "Craftable: " + RecipeBookManager.instance.CalculateMaxCraftableAmount(recipe).ToString();
        }

        if (ingredientSlots != null)
        {
            for (int i = 0; i < ingredientSlots.Count; i++)
            {
                if (i < recipe.ingredients.Count)
                {
                    ItemQuantity ingredient = recipe.ingredients[i];
                    ingredientSlots[i].UpdateSlot(ingredient);
                }
                else
                {
                    ingredientSlots[i].UpdateSlot(null);
                }
            }
        }
    }

    private void ClearDetails()
    {
        if (detailsRootObject != null)
        {
            detailsRootObject.SetActive(false);
        }

        if (headerIcon != null)
        {
            headerIcon.enabled = false;
            headerIcon.sprite = null;
        }
        if (recipeName != null) recipeName.text = "";
        if (recipeDescription != null) recipeDescription.text = "";
        if (storageText != null) storageText.text = "";
        if (craftableText != null) craftableText.text = "";

        if (ingredientSlots != null) 
        { 
            foreach (var slot in ingredientSlots)
            {
                if (slot != null)
                {
                    slot.ClearSlot();
                }
            }
        }
    }
}
