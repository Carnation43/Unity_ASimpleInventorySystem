using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the visual representation of a single inventory slot in the UI.
/// It takes an InventorySlot data object and updates its own UI components (icon, text, etc.) to match.
/// It also handles UI events like Select and Deselect to interact with other systems like the Tooltip.
/// </summary>
public class InventorySlotUI : MonoBehaviour, ISelectHandler
{
    [Header("Componentes")]
    [SerializeField] public Image icon;
    [SerializeField] TMP_Text stackCountText;
    [SerializeField] GameObject equippedIndicator;

    public InventorySlot slot; // The data object that this UI slot is currently representing

    public void Initialize(InventorySlot newItem)
    {
        slot = newItem;
        icon.gameObject.GetComponent<Image>().enabled = (newItem != null);

        if(newItem == null)
        {
            stackCountText.gameObject.SetActive(false);
            return;
        }

        icon.sprite = newItem.item.sprite;

        stackCountText.text = newItem.count.ToString();
        stackCountText.gameObject.SetActive(newItem.count > 1);

        if (equippedIndicator != null)
        {
            equippedIndicator.SetActive(newItem.isEquipped);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        // setup tooltip
        if (slot != null && eventData.selectedObject)
        {
           
            TooltipViewController.instance.ShowTooltip(slot, icon.rectTransform);

            // If the details panel is currently open, update its content
            if (DetailsContent.instance != null && DetailsContent.instance.IsChanged2Details)
            {
                DetailsContent.instance.SetupItemDetails(slot.item);
            }
        }
        else
        {
            Debug.Log("No Slot found !");
            TooltipViewController.instance.HideTooltip();
        }
    }
}
