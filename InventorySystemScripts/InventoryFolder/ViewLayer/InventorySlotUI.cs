using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Componentes")]
    [SerializeField] Image icon;
    [SerializeField] TMP_Text stackCountText;

    private Transform inventoryPanel; // inventory root

    public InventorySlot slot; // data structure

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
    }

    public void OnDeselect(BaseEventData eventData)
    {
        TooltipInstance.instance._trackedRectTransform = null;
        StartCoroutine(DelayedHide());
    }

    private IEnumerator DelayedHide()
    {
        // wait EventSystem updating
        yield return null;

        var current = EventSystem.current.currentSelectedGameObject;

        if (current != null && current.transform.IsChildOf(inventoryPanel))
        {
            // still in the inventory -> do not hide
            yield break;
        }

        // exit tyhe inventory -> hide tooltip
        TooltipInstance.instance.Hide();
        
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
            TooltipInstance.instance.setTooltip(slot);
            TooltipInstance.instance._trackedRectTransform = icon.rectTransform;
            TooltipInstance.instance.Show(icon.rectTransform);

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
