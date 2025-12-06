using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for a single ingredient in the Recipe Details View
/// </summary>
public class RecipeIngredientSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image ingredientIcon;
    [SerializeField] private TMP_Text ingredientText;
    [SerializeField] private TMP_Text storageAmountText;
    [SerializeField] private TMP_Text needAmountText;

    [Tooltip("Used for showing and hiding UIs")]
    [SerializeField] private GameObject ingredientSlotParent;

    public void UpdateSlot(ItemQuantity ingredient)
    {
        bool hasItem = (ingredient != null && ingredient.item != null);

        if (ingredientSlotParent != null)
        {
            ingredientSlotParent.SetActive(hasItem);
        }

        if (hasItem)
        {
            if (ingredientIcon != null)
            {
                ingredientIcon.sprite = ingredient.item.sprite;
                ingredientIcon.enabled = true;
            }

            if (ingredientText != null)
            {
                ingredientText.text = ingredient.item.itemName;
                ingredientText.enabled = true;
            }

            if (storageAmountText != null)
            {
                int storageCount = 0;
                if (InventoryManager.instance != null)
                {
                    storageCount = InventoryManager.instance.GetItemCount(ingredient.item);
                }

                storageAmountText.text = storageCount.ToString();
                storageAmountText.enabled = true;
            }

           if (needAmountText != null)
            {
                needAmountText.text = ingredient.count.ToString();
                needAmountText.enabled = true;
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        if (ingredientSlotParent != null)
        {
            ingredientSlotParent.SetActive(false);
        }

        if (ingredientIcon != null)
        {
            ingredientIcon.enabled = false;
            ingredientIcon.sprite = null;
        }

        if (needAmountText != null)
        {
            needAmountText.enabled = false;
            needAmountText.text = "";
        }

        if (ingredientText != null)
        {
            ingredientText.enabled = false;
            ingredientText.text = "";
        }

        if (storageAmountText != null)
        {
            storageAmountText.enabled = false;
            storageAmountText.text = "";
        }
    }
}
