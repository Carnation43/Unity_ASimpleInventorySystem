using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatsData : MonoBehaviour
{
    [Header("Level")]
    public int level = 1;
    public int currentGlows = 0;
    public int glowsToNextLevel = 100;

    [Header("Primary Attributes")]
    public int vigor = 1;
    public int attunement = 1;
    public int endurance = 1;
    public int vitality = 1;
    public int strength = 1;
    public int dexterity = 1;
    public int intelligence = 1;
    public int faith = 1;
    public int luck = 1;

    [Header("Base Stats")]
    public float maxHealth = 100;
    public float currentHealth = 100f;
    public float maxDefence = 10;
    public float physicalDefence = 10;
    public float attackPower;
}
