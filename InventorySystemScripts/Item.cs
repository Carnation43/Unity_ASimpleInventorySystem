using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New item", menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public int attack;
    public int defence;
    public int hp;
    public Sprite sprite;
    public ItemCategory category;
    public bool stackable = false;
}

public enum ItemCategory
{
    Key,
    Item,
    Weapon,
    Equipment,
    Accessory,
    Material,
}
