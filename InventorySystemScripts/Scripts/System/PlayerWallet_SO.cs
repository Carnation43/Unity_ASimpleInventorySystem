using InstanceResetToDefault;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerWallet", menuName = "Data/Player Wallet")]
public class PlayerWallet_SO : ScriptableObject, IResettableData
{
    public event Action OnInspirationChanged;
    public event Action OnGlowsChanged;

    [Header("Resources")]
    [Tooltip("used for unlocking recipe")]
    [SerializeField] private int _currentInspiration = 0;
    [SerializeField] private int _initialInspiration = 0;

    [Tooltip("used for upgrading, shopping and so on")]
    [SerializeField] private int _currentGlows = 0;
    [SerializeField] private int _initialGlows = 0;

    public int CurrentInspiration => _currentInspiration;
    public int CurrentGlows => _currentGlows;
    
    public bool TrySpendInspiration(int amount)
    {
        if (amount < 0) return false;

        if (_currentInspiration >= amount)
        {
            _currentInspiration -= amount;
            OnInspirationChanged?.Invoke();
            return true;
        }

        return false;
    }

    public void AddInspiration(int amount)
    {
        if (amount < 0) return;

        _currentInspiration += amount;
        OnInspirationChanged?.Invoke();
    }

    public bool TrySpendGlows(int amount)
    {
        if (_currentGlows >= amount)
        {
            _currentGlows -= amount;
            OnGlowsChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void AddGlows(int amount)
    {
        _currentGlows += amount;
        OnGlowsChanged?.Invoke();
    }

    public void ResetData()
    {
        _currentInspiration = _initialInspiration;
        _currentGlows = _initialGlows;
        OnInspirationChanged?.Invoke();
        OnGlowsChanged?.Invoke();
        Debug.Log("[PlayerWallet] Data Reset to Initial Values.");
    }
}
