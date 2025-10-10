using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatsController : MonoBehaviour
{
    public static CharacterStatsController instance;

    [Header("Listenint To")]
    [SerializeField] private CharacterStatsEventChannel statsChannel;

    [SerializeField] private CharacterStatsData _statsData;
    [SerializeField] private EquipmentManager _equipManager;

    public event Action OnStatsUpdated;
    public CharacterStatsData CurrentStatsData => _statsData;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != null) Destroy(instance);
    }

    private void OnEnable()
    {
        if (_equipManager != null)
        {
            _equipManager.OnEquipmentChanged += ReCalculateStats;
        }
        
        if(statsChannel != null)
        {
            statsChannel.OnStatsLeveledUp += ReCalculateStats;
        }
    }

    private void OnDisable()
    {
        if (_equipManager != null)
        {
            _equipManager.OnEquipmentChanged -= ReCalculateStats;
        }
        if (statsChannel != null)
        {
            statsChannel.OnStatsLeveledUp -= ReCalculateStats;
        }
    }

    private void Start()
    {
        ReCalculateStats();    
    }

    /// <summary>
    /// This is a simple calculation; it is not the focus of the design.
    /// </summary>
    private void ReCalculateStats()
    {
        if (_statsData == null) return;

        _statsData.attackPower = StatsCalculator.CalculateAttackPower(_statsData, _equipManager.equippedItems);
        _statsData.physicalDefence = StatsCalculator.CalculatePhysicalDefense(_statsData, _equipManager.equippedItems);

        _statsData.maxHealth = 100 + (_statsData.vigor * 10);

        OnStatsUpdated?.Invoke();
    }

    public void RestoreHealth(float amount)
    {
        if (_statsData == null) return;

        _statsData.currentHealth = Mathf.Min(_statsData.maxHealth, _statsData.currentHealth + amount);
        OnStatsUpdated?.Invoke();
    }
}
