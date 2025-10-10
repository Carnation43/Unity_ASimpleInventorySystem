using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    /// The points spent on handling consecutive upgrades
    /// </summary>
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

    public void ConfirmMultiLevelUp(Dictionary<StatType, int> pointsToAdd, int totalCost)
    {
        if (_statsData.currentGlows < totalCost)
        {
            Debug.Log("Fail to level up");
            return;
        }

        _statsData.currentGlows -= totalCost;

        int totalLevelsAdded = 0;

        foreach (var pair in pointsToAdd)
        {
            StatType stat = pair.Key; // The attributes that need to be upgraded currently
            int points = pair.Value;  // The points that need to be costed

            if (points <= 0) continue;

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

        if(statsChannel != null)
        {
            statsChannel.RaiseStatsLeveledUpEvent();
        }
    }

    private int CalculateCostForSingleLevel(int level)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(level, costExponent));
    }
}
