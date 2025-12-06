using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Process computational logic
/// </summary>
public class StatsCalculator
{
    private const float MOCK_STRENGTH_SCALING = 0.6f;
    private const float MOCK_DEXTERITY_SCALING = 0.4f;

    /// <summary>
    /// Calculate the final attack power after equipping the equipment
    /// </summary>
    /// <param name="statsData">Character base stats</param>
    /// <param name="equippedItems">A dictionary that contains equipped items</param>
    /// <returns>Final attack power</returns>
    public static float CalculateAttackPower(CharacterStatsData statsData, Dictionary<EquipmentSlotType, Item> equippedItems)
    {
        if (statsData == null) return 0;

        float totalEquipmentAttack = 0f;

        // 1.Accumulate the attack power increased by all equipment
        foreach (var item in equippedItems.Values)
        {
            if (item != null) totalEquipmentAttack += item.attack;
        }

        // 2.Check if equipped with weapon
        if (equippedItems.ContainsKey(EquipmentSlotType.Weapon))
        {
            float attributeBonus = (statsData.strength * MOCK_STRENGTH_SCALING) + (statsData.dexterity * MOCK_DEXTERITY_SCALING);
            return totalEquipmentAttack + attributeBonus;
        }
        else
        {
            // Unarmed attack power
            float baseAttackPower = 5 + (statsData.strength * 0.5f);
            return baseAttackPower + totalEquipmentAttack;
        }
    }

    public static float CalculatePhysicalDefense(CharacterStatsData statsData, Dictionary<EquipmentSlotType, Item> equippedItems)
    {
        if (statsData == null) return 0;

        float baseDefense = (statsData.level * 0.05f) + (statsData.vitality * 0.08f);

        float totalEquipmentDefence = 0f;

        // 1.Accumlate the defense increased by all equipment
        foreach (var item in equippedItems.Values)
        {
            if (item != null) totalEquipmentDefence += item.defence;
        }

        return baseDefense * totalEquipmentDefence;
    }
}
