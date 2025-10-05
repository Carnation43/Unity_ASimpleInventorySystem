using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Only focus on how to display the content of the Tooltip
/// </summary>
public class TooltipView : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text amount;
    [SerializeField] public Button detailsButton;
    [SerializeField] public Button equipButton;
    [SerializeField] private TMP_Text equipButtonText;

    [Header("Components - Stats")]
    [SerializeField] private GameObject attackGo;
    [SerializeField] private TMP_Text attackValue;
    [SerializeField] private GameObject defenceGo;
    [SerializeField] private TMP_Text defenceValue;
    [SerializeField] private GameObject hpGo;
    [SerializeField] private TMP_Text hpValue;
    [SerializeField] private TMP_Text description;

    public void SetTooltip(InventorySlot slot)
    {
        title.text = slot.item.itemName;
        amount.text = "Amount: " + slot.count.ToString();

        // stats
        attackGo.SetActive(slot.item.attack > 0);
        attackValue.text = slot.item.attack.ToString();
        defenceGo.SetActive(slot.item.defence > 0);
        defenceValue.text = slot.item.defence.ToString();
        hpGo.SetActive(slot.item.hp != 0);
        if (slot.item.hp > 0)
        {
            hpValue.text = "+" + slot.item.hp.ToString();
            hpValue.color = new Color(0, 0.6f, 0, 1);
        }
        else
        {
            hpValue.text = slot.item.hp.ToString();
            hpValue.color = new Color(0.6f, 0, 0, 1);
        }

        description.text = slot.item.GeneralDescription;

        // Consumable = 1 | weapon = 2 | Equipment = 3 | Accessory = 4 | Material = 5 |

        bool isEquippable = slot.item.isEquippable;
        equipButton.gameObject.SetActive(true);
        if (isEquippable)
        {
            equipButtonText.text = slot.isEquipped ? "Unequip(J)" : "Equip(J)";
        }
        else
        {
            switch (slot.item.category)
            {
                case ItemCategory.Consumable: equipButtonText.text = "Consume (J)"; break;
                case ItemCategory.Material: equipButtonText.text = "Craft (J)"; break;
                default: equipButton.gameObject.SetActive(false); break;
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }
}
