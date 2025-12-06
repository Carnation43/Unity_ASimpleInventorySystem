using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Listen to the ItemActionEventChannel to receive requests from the UI Layer, 
/// then execute game logic (such as equipping, consuming).
/// </summary>
public class ItemActionManager : MonoBehaviour
{
    [Header("Listening To")]
    [SerializeField] private ItemActionEventChannel itemActionChannel;

    [Header("Broadcasting On")]
    [SerializeField] private AudioCueEventChannel uiAudioChannel;
    [SerializeField] private CharacterPanelVfxChannel characterVfxChannel;

    [Header("Audio Cues")]
    [SerializeField] private AudioCueSO consumeCue;
    [SerializeField] private AudioCueSO equipCue;
    [SerializeField] private AudioCueSO craftCue; // TODO:
    [SerializeField] private AudioCueSO confirmCue;

    private void OnEnable()
    {
        if (itemActionChannel != null)
        {
            itemActionChannel.OnActionRequested += HandleActionRequest;
        }
    }

    private void OnDisable()
    {
        if (itemActionChannel != null)
        {
            itemActionChannel.OnActionRequested -= HandleActionRequest;
        }
    }

    /// <summary>
    /// Distribute tasks according to the type of request
    /// </summary>
    /// <param name="request">A structure containing target item slots and operation types</param>
    private void HandleActionRequest(ItemActionRequest request)
    {
        if (request.Slot == null) return;

        switch (request.ActionType)
        {
            case RadialMenuActionType.Equip:
            case RadialMenuActionType.UnEquip:
                PerformEquipAction(request.Slot);
                break;

            case RadialMenuActionType.Use:
                PerformConsumeAction(request.Slot);
                break;

            case RadialMenuActionType.ShowDetails:
                PerformDetailsAction();
                break;

            case RadialMenuActionType.Drop:
                PerformDropAction();
                break;

            case RadialMenuActionType.Sort:
                PerformSortAction();
                break;

            case RadialMenuActionType.Fix:
                PerformFixAction();
                break;

            case RadialMenuActionType.Enhance:
                PerformEnhanceAction();
                break;

            case RadialMenuActionType.Combine:
                PerformCombineAction(request.Slot);
                break;

            case RadialMenuActionType.Craft:
                PerformCraftAction(request.Slot);
                break;

            case RadialMenuActionType.Present:
                PerformPresentAction();
                break;
        }
    }

    /// <summary>
    /// Execute the logic of equipping or unequipping
    /// </summary>
    /// <param name="slot">Inventory Slot</param>
    private void PerformEquipAction(InventorySlot slot)
    {
        if (slot.isEquipped)
        {
            // --- unequip ---
            uiAudioChannel?.RaiseEventWithPitch(equipCue, 0.85f);
            EquipmentManager.instance.UnEquip(slot.item.equipmentSlotType);
        }
        else
        {
            // --- equip ---
            uiAudioChannel?.RaiseEventWithPitch(equipCue, 1.15f);
            // characterVfxChannel?.RaiseEvent(CharacterVfxType.Equip);
            EquipmentManager.instance.Equip(slot);
        }
    }

    /// <summary>
    /// the logic of consuming items
    /// </summary>
    /// <param name="slot"></param>
    private void PerformConsumeAction(InventorySlot slot)
    {
        // --- Consume ---
        uiAudioChannel?.RaiseEvent(consumeCue);
        characterVfxChannel?.RaiseEvent(CharacterVfxType.Consume);
        CharacterStatsController.instance.RestoreHealth(slot.item.hp);
        InventoryManager.instance.RemoveItem(slot);
    }

    /// <summary>
    /// display more details about items
    /// </summary>
    private void PerformDetailsAction()
    {
        uiAudioChannel?.RaiseEvent(confirmCue);
        DetailsContent.instance.ToggleByInput();
    }

    /// <summary>
    /// TODO: DROP LOGIC
    /// </summary>
    private void PerformDropAction()
    {
        uiAudioChannel?.RaiseEvent(confirmCue);
        Debug.Log("-------- Drop --------");
    }

    /// <summary>
    /// TODO: SORT LOGIC
    /// </summary>
    private void PerformSortAction()
    {
        uiAudioChannel?.RaiseEvent(confirmCue);
        Debug.Log("-------- Sort --------");
    }

    /// <summary>
    /// TODO: FIX LOGIC
    /// </summary>
    private void PerformFixAction()
    {
        uiAudioChannel?.RaiseEvent(confirmCue);
        Debug.Log("----------  Fix  ----------");
    }

    /// <summary>
    /// TODO: ENHANCE LOGIC
    /// </summary>
    private void PerformEnhanceAction()
    {
        uiAudioChannel?.RaiseEvent(confirmCue);
        Debug.Log("--------- Enhance ---------");
    }

    /// <summary>
    /// Associated with crafting system and used for weapon
    /// </summary>
    /// <param name="slot"></param>
    private void PerformCombineAction(InventorySlot slot)
    {
        uiAudioChannel?.RaiseEvent(confirmCue);
        CraftingView.instance.AddItemToCraft(slot);
        Debug.Log("--------- Combine ---------");
    }

    /// <summary>
    /// Add items to craft panel
    /// </summary>
    /// <param name="slot"></param>
    private void PerformCraftAction(InventorySlot slot)
    {
        uiAudioChannel?.RaiseEvent(confirmCue);
        CraftingView.instance.AddItemToCraft(slot);
    }

    /// <summary>
    /// TODO: PRESENT LOGIC
    /// </summary>
    private void PerformPresentAction()
    {
        uiAudioChannel?.RaiseEvent(confirmCue);
        Debug.Log("--------- Present ---------");
    }
}
