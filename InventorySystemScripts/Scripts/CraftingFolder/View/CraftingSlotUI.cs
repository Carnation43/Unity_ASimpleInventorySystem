using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Control the UI performance and interaction of individual slots
/// within the crafting panel
/// </summary>
public class CraftingSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private GameObject selectImage;
    [SerializeField] private GameObject singleItemBorder;
    [SerializeField] private GameObject multiItemBorder;
    [SerializeField] private ParticleSystem craftingPoof;

    [Header("Slot Settings")]
    [SerializeField] private bool isOutputSlot = false;
    public int slotIndex;

    private InventorySlot _currentSlotData;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = icon.GetComponent<CanvasGroup>();
        if (craftingPoof != null)
            craftingPoof.gameObject.SetActive(false);
        UpdateSlot(null);
    }

    public void PlayCraftingEffect()
    {
        if (craftingPoof != null)
        {
            craftingPoof.gameObject.SetActive(true);
            craftingPoof.Play();
        }
    }

    // Update the slot content
    public void UpdateSlot(InventorySlot slotData)
    {
        _currentSlotData = slotData;

        bool hasItem = slotData != null && slotData.item != null;

        icon.gameObject.SetActive(hasItem);

        if (hasItem)
        {
            icon.sprite = slotData.item.sprite;
            bool isMulti = slotData.count > 1;

            singleItemBorder.SetActive(!isMulti);
            multiItemBorder.SetActive(isMulti);

            amountText.gameObject.SetActive(isMulti);
            if (isMulti)
            {
                amountText.text = slotData.count.ToString();
            }
        }
        else
        {
            singleItemBorder.SetActive(true);
            multiItemBorder.SetActive(false);
            amountText.gameObject.SetActive(false);
        }
    }

    public void SetPreviewState(bool isConfirmed)
    {
        if (_canvasGroup != null && isOutputSlot)
        {
            _canvasGroup.alpha = isConfirmed ? 1f : 0.5f;
        }
    }
}
