using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Character Stats Event Channel")]
public class CharacterStatsEventChannel : ScriptableObject
{
    public event Action OnStatsLeveledUp;

    /// <summary>
    /// Call this method to broadcast after the upgrade.
    /// </summary>
    public void RaiseStatsLeveledUpEvent()
    {
        OnStatsLeveledUp?.Invoke();
    }
}
