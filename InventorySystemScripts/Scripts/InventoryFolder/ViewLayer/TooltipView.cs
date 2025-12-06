using InstanceResetToDefault;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Only focus on how to display the content of the Tooltip
/// Implements IResettableUI to clear data when the tooltip is closed
/// </summary>
public class TooltipView : MonoBehaviour, IResettableUI
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

    [Header("Components - CorrectionValues")]
    [SerializeField] private TMP_Text attackCorrectionValue;
    [SerializeField] private TMP_Text defenceCorrectionValue;

    // const color
    private readonly Color positiveColor = new Color(0, 0.6f, 0, 1);
    private readonly Color negativeColor = new Color(0.6f, 0, 0, 1);
    private readonly Color neutralColor = Color.grey;

    public void SetTooltip(InventorySlot slot)
    {
        Item item = slot.item;

        title.text = slot.item.itemName;
        amount.text = "Amount: " + slot.count.ToString();
        description.text = slot.item.GeneralDescription;

        // stats
        attackGo.SetActive(item.attack != 0);
        attackValue.text = item.attack.ToString();
        defenceGo.SetActive(item.defence != 0);
        defenceValue.text = item.defence.ToString();
        hpGo.SetActive(item.hp != 0);
        hpValue.text = item.hp > 0 ? $"+{item.hp}" : item.hp.ToString();
        hpValue.color = item.hp > 0 ? positiveColor : negativeColor;

        if (item.isEquippable)
        {
            CharacterStatsData statsData = CharacterStatsController.instance.CurrentStatsData;

            // Perform prediction operations here
            Dictionary<EquipmentSlotType, Item> predictedEquipment = new Dictionary<EquipmentSlotType, Item>(EquipmentManager.instance.equippedItems);

            if (slot.isEquipped)
            {
                predictedEquipment.Remove(item.equipmentSlotType);
            }
            else
            {
                predictedEquipment[item.equipmentSlotType] = item;
            }

            float predictedAttack = StatsCalculator.CalculateAttackPower(statsData, predictedEquipment);
            float predictedDefense = StatsCalculator.CalculatePhysicalDefense(statsData, predictedEquipment);
            // Update UI
            UpdateStatChangeUI(attackCorrectionValue, predictedAttack - statsData.attackPower);
            UpdateStatChangeUI(defenceCorrectionValue, predictedDefense - statsData.physicalDefence);
        }
        else
        {
            attackCorrectionValue.gameObject.SetActive(false);
            defenceCorrectionValue.gameObject.SetActive(false);
        }
        
        equipButton.gameObject.SetActive(true);
        if (item.isEquippable)
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

    private void UpdateStatChangeUI(TMP_Text correctionText, float correctionValue)
    {
        string formatValue = correctionValue.ToString("F1");
        correctionText.gameObject.SetActive(true);

        if(correctionValue > 0)
        {
            correctionText.text = $"+{formatValue}";
            correctionText.color = positiveColor;
        }
        else if(correctionValue < 0)
        {
            correctionText.text = $"{formatValue}";
            correctionText.color = negativeColor;
        }
        else 
        {
            correctionText.text = "0";
            correctionText.color = neutralColor; 
        }
    }

    public void ResetUI()
    {
        if (title != null) title.text = "";
        if (amount != null) amount.text = "";
        if (description != null) description.text = "";

        if (attackGo != null) attackGo.SetActive(false);
        if (defenceGo != null) defenceGo.SetActive(false);
        if (hpGo != null) hpGo.SetActive(false);

        if (attackCorrectionValue != null) attackCorrectionValue.gameObject.SetActive(false);
        if (defenceCorrectionValue != null) defenceCorrectionValue.gameObject.SetActive(false);

        if (equipButton != null) equipButton.gameObject.SetActive(false);
    }
}
