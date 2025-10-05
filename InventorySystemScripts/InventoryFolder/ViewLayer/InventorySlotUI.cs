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
public class InventorySlotUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Componentes")]
    [SerializeField] Image icon;
    [SerializeField] TMP_Text stackCountText;
    [SerializeField] GameObject equippedIndicator;

    private Transform inventoryPanel; // inventory root, used for tooltip logic

    public InventorySlot slot; // The data object that this UI slot is currently representing

    private void Awake()
    {
        inventoryPanel = transform.parent;
    }

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

    public void OnDeselect(BaseEventData eventData)
    {
        if (TooltipRaycastTarget.IsPointerOver)
        {
            StartCoroutine(ReselectAfterFrame());
        }
        // TooltipInstance.instance._trackedRectTransform = null;
        StartCoroutine(DelayedHide());
    }

    /// <summary>
    /// Avoid interrupting the atomic operation of selecting the current item
    /// </summary>
    private IEnumerator ReselectAfterFrame()
    {
        // Wait until the camera and UI rendering of the current frame
        // is completed before reselecting the current item.
        yield return new WaitForEndOfFrame();

        if (EventSystem.current.currentSelectedGameObject == null && gameObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    /// <summary>
    /// Avoid possible tooltip flickering effects
    /// </summary>
    private IEnumerator DelayedHide()
    {
        // wait EventSystem updating
        yield return null;

        var current = EventSystem.current.currentSelectedGameObject;

        // Determine whether cursor is in the inventory panel.
        if (current != null && current.transform.IsChildOf(inventoryPanel))
        {
            // still in the inventory -> do not hide
            yield break;
        }

        // exit tyhe inventory -> hide tooltip
        TooltipViewController.instance.HideTooltip();
        
    }

    public void OnSelect(BaseEventData eventData)
    {
        // setup tooltip
        if (slot != null && eventData.selectedObject)
        {
            /** Debug 
              *  Debug.Log($"eventData.selectedObject: {eventData.selectedObject?.name} " +
              *  //$"(ID={eventData.selectedObject?.GetInstanceID()})");

              *  //  Debug.Log($"this.gameObject: {gameObject.name} " +
              *  //            $"(ID={gameObject.GetInstanceID()})");

              *  //  Debug.Log($"EventSystem.current.currentSelectedGameObject: " +
              *  //            $"{EventSystem.current.currentSelectedGameObject?.name} " +
              *  //            $"(ID={EventSystem.current.currentSelectedGameObject?.GetInstanceID()})");
              *  // Debug.Log("current select: " + slot.item.name);
              */

            // active tooltip
            if (TooltipViewController.instance != null)
            {
                TooltipViewController.instance.ShowTooltip(slot, icon.rectTransform);
            }

            // If the details panel is currently open, update its content
            if (DetailsContent.instance != null && DetailsContent.instance.IsChanged2Details)
            {
                DetailsContent.instance.SetupItemDetails(slot.item);
            }
        }
        else
        {
            Debug.Log("No Slot found !");
        }
    }
}
