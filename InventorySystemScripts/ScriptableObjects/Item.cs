using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RadialMenuActionType
{
    ShowDetails, Drop, Sort,                // Common
    Use, Combine,                           // Consumable
    Equip, UnEquip, Enhance, Fix,           // Equipment
    Present,                                // Key
    Craft,                                  // Material
    None
}

[CreateAssetMenu(fileName = "New item", menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject
{
    [Header("Icon Database")]
    [SerializeField] private RadialMenuIconDB iconDB;

    [Header("Basic Information")]
    public string itemName;
    public Sprite sprite;
    public ItemCategory category;
    public bool stackable = false;
    public bool isEquipped = false;
    public bool isEquippable = false;
    public EquipmentSlotType equipmentSlotType;

    [Header("Attributes")]
    public float attack;
    public float defence;
    public float hp;

    [Header("Descriptions")]
    [TextArea(3, 5)]
    public string specificDescription;
    [TextArea(5, 10)]
    public string storyDescription;

    // readonly
    public string GeneralDescription
    {
        get
        {
            switch (category)
            {
                case ItemCategory.Weapon:
                    return "The maiden thrusts her blade into tenderness.";
                case ItemCategory.Equipment:
                    return "The maiden conceals her sobs.";
                case ItemCategory.Consumable:
                    return "The maiden tastes sorrow.";
                case ItemCategory.Key:
                    return "The maiden fades into forgetting.";
                case ItemCategory.Accessory:
                    return "The maiden wears a smile that masks her woe.";
                case ItemCategory.Material:
                    return "The maiden dissects life piece by piece.";
                default:
                    return "null";
            }
        }
    }

    /// <summary>
    /// Return different sets of pictures and instructions according to the types of items
    /// </summary>
    public List<(Sprite icon, RadialMenuActionType actionType)> GetActionRequests()
    {
        var requests = new List<(Sprite icon, RadialMenuActionType actionType)>();

        if (iconDB == null)
        {
            Debug.LogError($"Item [{itemName}] does not have an iconDB");
            return requests;
        }

        switch (category)
        {
            case ItemCategory.Consumable:
                requests.Add((iconDB.useIcon, RadialMenuActionType.Use));
                // requests.Add((iconDB.combineIcon, RadialMenuActionType.Combine));
                break;

            case ItemCategory.Weapon:
                requests.Add((iconDB.enhanceIcon, RadialMenuActionType.Enhance));
                requests.Add((iconDB.combineIcon, RadialMenuActionType.Combine));
                requests.Add((iconDB.equipIcon, RadialMenuActionType.Equip));
                requests.Add((iconDB.unequipIcon, RadialMenuActionType.UnEquip));
                break;

            case ItemCategory.Equipment:
            case ItemCategory.Accessory:
                requests.Add((iconDB.enhanceIcon, RadialMenuActionType.Enhance));
                requests.Add((iconDB.equipIcon, RadialMenuActionType.Equip));
                requests.Add((iconDB.unequipIcon, RadialMenuActionType.UnEquip));
                break;

            case ItemCategory.Key:
                requests.Add((iconDB.presentIcon, RadialMenuActionType.Present));
                break;

            case ItemCategory.Material:
                requests.Add((iconDB.craftIcon, RadialMenuActionType.Craft));
                break;
        }

        // Add common interaction
        requests.Add((iconDB.detailsIcon, RadialMenuActionType.ShowDetails));
        requests.Add((iconDB.sortIcon, RadialMenuActionType.Sort));
        requests.Add((iconDB.dropIcon, RadialMenuActionType.Drop));

        return requests;
    }
}

public enum ItemCategory
{
    Key,
    Consumable,
    Weapon,
    Equipment,
    Accessory,
    Material,
    All
}
