using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Players upgrade attributes
/// </summary>
public enum StatType
{
    Vigor,
    Attunement,
    Endurance,
    Vitality,
    Strength,
    Dexterity,
    Intelligence,
    Faith,
    Luck
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Broadcasting On")]
    [SerializeField] private CharacterStatsEventChannel statsChannel;

    [Header("Dependencies")]
    [SerializeField] private PlayerWallet_SO _wallet;
    [SerializeField] private CharacterStatsData _statsData;

    [Header("Leveling Formula")]
    [SerializeField] private float costExponent = 1.5f;
    [SerializeField] private int baseCost = 100;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    /// <summary>
    /// Calculate the cost required to level up starting from the current level.
    /// </summary>
    /// <param name="startingLevel">Player's current level</param>
    /// <param name="levelsToAdd">The number of levels to be upgraded</param>
    /// <returns>Total cost</returns>
    public int CalculateCostForLevels(int startingLevel, int levelsToAdd)
    {
        if (levelsToAdd <= 0) return 0;

        int totalCost = 0;
        for (int i = 0; i < levelsToAdd; i++)
        {
            int currentLevel = startingLevel + i;
            totalCost += CalculateCostForSingleLevel(currentLevel);
        }
        return totalCost;
    }

    /// <summary>
    /// Deduct glow to update character data according to the assigned points
    /// </summary>
    /// <param name="pointsToAdd">A dictionary contains points to be added in the attributes</param>
    public void ConfirmMultiLevelUp(Dictionary<StatType, int> pointsToAdd, int totalCost)
    {
        if (_wallet == null)
        {
            Debug.LogError("[LevelManager] unbound wallet !");
            return;
        }

        if (_wallet.TrySpendGlows(totalCost))
        {
            // 3. 扣费成功，执行升级逻辑
            ApplyLevelUp(pointsToAdd);
        }
        else
        {
            Debug.Log("Glows 不足，无法升级");
        }
        
    }

    private int CalculateCostForSingleLevel(int level)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(level, costExponent));
    }

    private void ApplyLevelUp(Dictionary<StatType, int> pointsToAdd)
    {
        int totalLevelsAdded = 0;

        foreach (var pair in pointsToAdd)
        {
            StatType stat = pair.Key; // The attributes that need to be upgraded currently
            int points = pair.Value;  // The points that need to be costed

            if (points <= 0) continue; // Skip attributes without upgrading

            totalLevelsAdded += points;

            switch (stat)
            {
                case StatType.Vigor: _statsData.vigor += points; break;
                case StatType.Attunement: _statsData.attunement += points; break;
                case StatType.Endurance: _statsData.endurance += points; break;
                case StatType.Vitality: _statsData.vitality += points; break;
                case StatType.Strength: _statsData.strength += points; break;
                case StatType.Dexterity: _statsData.dexterity += points; break;
                case StatType.Intelligence: _statsData.intelligence += points; break;
                case StatType.Faith: _statsData.faith += points; break;
                case StatType.Luck: _statsData.luck += points; break;
            }
        }

        _statsData.level += totalLevelsAdded;

        if (statsChannel != null)
        {
            statsChannel.RaiseStatsLeveledUpEvent();
        }
    }
}
