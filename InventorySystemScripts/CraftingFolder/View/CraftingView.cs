using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Responsible for handling all UI interactions, animations, sound effects, and visual effects of the composite panel
/// 
/// </summary>
public class CraftingView : MonoBehaviour
{
    public static CraftingView instance;

    [Header("Input Listening")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("Animator Reference")]
    [Tooltip("controls the crafting progress bar animation")]
    [SerializeField] private CraftingAnimator craftingAnimator; 
    [Tooltip("The parent transform for particle effects to ensure they render on the correct UI canvas.")]
    [SerializeField] private Transform animationParent; 
    [Tooltip("used to find the target tab transform for the item retrieval animation")]
    [SerializeField] private TabsManager tabsManager; 
    [Tooltip("The particle effect prefab for the animation of items flying to the inventory")]
    [SerializeField] private GameObject itemFlyVFXPrefab;

    [Header("SFX Broadcasting On")]
    [SerializeField] private AudioCueEventChannel uiAudioChannel;
    [SerializeField] private AudioCueSO craftSuccessCue;
    [SerializeField] private AudioCueSO retrieveCue;

    [Header("UI Components")]
    [Tooltip("A list of the 3 UI components for ingredient input slots.")]
    [SerializeField] private List<CraftingSlotUI> inputSlots;
    [Tooltip("The UI component for the crafting result output slot")]
    [SerializeField] private CraftingSlotUI outputSlot;
    [SerializeField] private TMP_Text promptText;

    // A list of GameObjects that can be navigated
    private List<GameObject> _navigatableSlots;     // slots can be navigated

    /// <summary>
    /// Defines the internal states of the crafting panel 
    /// to manage different interaction logic
    /// </summary>
    private enum CraftingState
    {
        Idle,
        ReadyToCraft,
        Holding,
        PendingRetrieval
    }
    private CraftingState _currentState = CraftingState.Idle;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(instance);

        // Initialize the list of navigatable UI slots
        InitializeNavigatableSlots();
    }

    private void OnEnable()
    {
        SetState(CraftingState.Idle);

        if (inputChannel != null)
        {
            inputChannel.OnConfirmStarted += HandleConfirmStarted;
            inputChannel.OnConfirmPerformed += HandleConfirmPerformed;
            inputChannel.OnConfirmCanceled += HandleConfirmCanceled;
            inputChannel.OnShowDetails += HandleCancelInput;
        }
    }

    private void OnDisable()
    {
        // Auto-retrieved crafted items on CraftingView disable.
        if (CraftingManager.instance.CraftedItemSlot != null)
        {
            CraftingManager.instance.RetrieveCraftedItems();
        }
        if (inputChannel != null)
        {
            inputChannel.OnConfirmStarted -= HandleConfirmStarted;
            inputChannel.OnConfirmPerformed -= HandleConfirmPerformed;
            inputChannel.OnConfirmCanceled -= HandleConfirmCanceled;
            inputChannel.OnShowDetails -= HandleCancelInput;
        }
    }

    // --- STATE MANAGEMENT ---

    /// <summary>
    /// Change the state of the crafting panel 
    /// and logs the change for debugging purposes.
    /// </summary>
    /// <param name="newState">The new state to transition to</param>
    private void SetState(CraftingState newState)
    {
        if (_currentState == newState) return;
        _currentState = newState;
    }

    // --- INPUT HANDLERS ---

    /// <summary>
    /// Handles the moment the confirm button is pressed down.
    /// </summary>
    private void HandleConfirmStarted(InputAction.CallbackContext context)
    {
        // Ensure this logic only runs when the crafting panel has focues.
        if (MenuController.instance.currentFocus != MenuController.MenuFocus.Crafting) return;

        // check if the currently selected UI is the output slot
        bool isOutput = EventSystem.current.currentSelectedGameObject.GetComponent<CraftingSlotUI>() == outputSlot;

        switch (_currentState)
        {
            case CraftingState.Idle:
                // return if no recipes
                if (CraftingManager.instance.MatchedRecipe == null) return;

                // if an input slot is selected, ready to craft
                if (!isOutput)
                {
                    inputChannel.RaiseGlobalInputLockEvent(true);
                    SetState(CraftingState.ReadyToCraft);
                }
                break;

            case CraftingState.PendingRetrieval:
                if (isOutput)
                {
                    RetrieveItems();
                }
                break;
        }
    }

    /// <summary>
    /// Handle the completion of a "Hold" interaction on the confirm button.
    /// </summary>
    private void HandleConfirmPerformed(InputAction.CallbackContext context)
    {
        // Only respond to a hold interaction if we are in the ReadyToCraft state
        if (_currentState == CraftingState.ReadyToCraft)
        {
            if (CraftingManager.instance.CalculateMaxCraftableAmount() > 1 && 
                context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
            {
                craftingAnimator.StartLongCraftAnimation();
                SetState(CraftingState.Holding);
            }
        }
    }

    /// <summary>
    /// Handle the moment the confirm button is released.
    /// This triggers for both taps and the end of a hold.
    /// </summary>
    private void HandleConfirmCanceled(InputAction.CallbackContext context)
    {
        if (MenuController.instance.currentFocus != MenuController.MenuFocus.Crafting) return;
        inputChannel.RaiseGlobalInputLockEvent(false);

        switch (_currentState)
        {
            case CraftingState.ReadyToCraft:
                // If it was a short tap, craft a signle item
                if (CraftingManager.instance.CalculateMaxCraftableAmount() <= 1)
                {
                    CraftSingleItem();
                }
                else // if a "craft all" was possible but the user tapped, do nothing
                {
                    SetState(CraftingState.Idle);
                }
                break;

            case CraftingState.Holding:
                // if the hold animation completed before release.
                if (craftingAnimator.FillAmount >= 1.0f)
                {
                    CraftAllItems();
                }
                else // hold was released
                {
                    SetState(CraftingState.Idle);
                }
                craftingAnimator.CancelLongCraftAnimation();
                break;
        }
    }

    /// <summary>
    /// Handles the "Cancel" to take back a single item from an input slot
    /// </summary>
    private void HandleCancelInput(InputAction.CallbackContext context)
    {
        // Only trigger on the initial press and only when in the Idle state.
        if (!context.started || _currentState != CraftingState.Idle) return;

        var currentSelected = EventSystem.current.currentSelectedGameObject;
        if (currentSelected == null) return;

        var slotUI = currentSelected.GetComponent<CraftingSlotUI>();

        // Ensure the selected object is a valid input slot.
        if (slotUI != null && currentSelected != outputSlot.gameObject)
        {
            // Request the CraftingManager to remove a single item from the data.
            Item returnedItem = CraftingManager.instance.RemoveSingleItemFromSlot(slotUI.slotIndex);

            // If an item was returned, add it back to inventory and update the UI.
            if (returnedItem != null)
            {
                InventoryManager.instance.AddItem(returnedItem);

                UpdateAllSlotsUI();
            }
        }
    }

    // --- UI NAVIGATION & UPDATES ---

    /// <summary>
    /// Initialize the list of UI elements that can be focused by naivgation controls.
    /// </summary>
    private void InitializeNavigatableSlots()
    {
        _navigatableSlots = new List<GameObject>();
        // TSource: InventorySlot - TResult: GameObject
        // Generate a sequence containing GameObject and add it to _navigatableSlots
        _navigatableSlots.AddRange(inputSlots.Select(s => s.gameObject));

        // The output slot is only navigatable when there is an item to retrieve.
        if (_currentState == CraftingState.PendingRetrieval)
        {
            _navigatableSlots.Add(outputSlot.gameObject);
        }
    }

    /// <summary>
    /// Public method for other systems to get the
    /// current list of navigatable slots.
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetNavigatableSlots()
    {
        // Rebuild the navigatable list
        InitializeNavigatableSlots();
        return _navigatableSlots;
    }

    /// <summary>
    /// Add ingredients from the inventory to craft
    /// </summary>
    /// <param name="itemSlotFromInventory">items need to craft</param>
    public void AddItemToCraft(InventorySlot itemSlotFromInventory)
    {
        // Force the player to retrieve items before adding new ones
        if (_currentState == CraftingState.PendingRetrieval)
        {
            RetrieveItems();
        }

        int targetSlotIndex = -1;

        // 1. Try to find an existing stack of the same item
        for (int i = 0; i < inputSlots.Count; i++)
        {
            var existingSlot = CraftingManager.instance.CraftingSlots[i];

            if (existingSlot != null && existingSlot.item == itemSlotFromInventory.item)
            {
                targetSlotIndex = i;
                break;
            }
        }

        // 2. if no stack exists, seek the first empty slot
        if (targetSlotIndex == -1)
        {
            for (int i = 0; i < inputSlots.Count; i++)
            {
                if (CraftingManager.instance.CraftingSlots[i] == null)
                {
                    targetSlotIndex = i;
                    break;
                }
            }
        } 

        // 3. find a suitable slot
        if (targetSlotIndex != -1)
        {
            // Create a new slot with a count of 1 to add to the grid.
            InventorySlot slotForCrafting = new InventorySlot(itemSlotFromInventory.item) { count = 1 };
            // Attempt to add it to the CraftingManaget's data
            bool success = CraftingManager.instance.AddItemToSlot(targetSlotIndex, slotForCrafting);

            if (success)
            {
                // If success, remove one item from the player's inventory
                InventoryManager.instance.RemoveItem(itemSlotFromInventory);
                UpdateAllSlotsUI();
            }
        }
        else
        {
            Debug.Log("The production slot is full.");
        }
    }

    // --- CRAFTING & RETRIEVAL LOGIC ---

    /// <summary>
    /// Executes the logic for crafting a single item.
    /// </summary>
    private void CraftSingleItem()
    {
        uiAudioChannel.RaiseEvent(craftSuccessCue);
        craftingAnimator.PlaySingleCraftAnimation();
        CraftingManager.instance.CraftItem();
        SetState(CraftingState.PendingRetrieval);
        UpdateAllSlotsUI();
        outputSlot.PlayCraftingEffect();
        EventSystem.current.SetSelectedGameObject(outputSlot.gameObject);
    }

    /// <summary>
    /// Executes the logic for crafting all possible items based on available ingreidents.
    /// </summary>
    private void CraftAllItems()
    {
        uiAudioChannel.RaiseEvent(craftSuccessCue);
        CraftingManager.instance.CraftAllItems();
        SetState(CraftingState.PendingRetrieval); 
        UpdateAllSlotsUI();
        outputSlot.PlayCraftingEffect();
        EventSystem.current.SetSelectedGameObject(outputSlot.gameObject);
    }

    /// <summary>
    /// Retrieve all items from output slot and moves them to the inventory
    /// </summary>
    private void RetrieveItems()
    {
        InventorySlot craftedSlot = CraftingManager.instance.CraftedItemSlot;
        if (craftedSlot == null) return;

        Item itemToAnimate = craftedSlot.item;
        int itemCount = craftedSlot.count;

        // Start coroutines for auido and visual feedback.
        StartCoroutine(PlayRetrieveSoundCoroutine(itemCount));
        StartCoroutine(AnimateRetrieveWithVFX(itemToAnimate, itemCount));

        // Tell the data manager to move the items.
        CraftingManager.instance.RetrieveCraftedItems();
        // Refresh the UI
        UpdateAllSlotsUI();
        // Move focus back to the first input slot for convenience
        EventSystem.current.SetSelectedGameObject(inputSlots[0].gameObject);
        // return to idle state
        SetState(CraftingState.Idle);
        UpdatePromptText("Craft(J)");
    }

    private IEnumerator PlayRetrieveSoundCoroutine(int count)
    {
        // Limit the number of sounds played
        int playCount = Mathf.Min(count, 5);

        for (int i = 0; i < playCount; i++)
        {
            // play one shot
            uiAudioChannel.RaiseEvent(retrieveCue);
            // wait for a short duration
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Coroutine to play a particle effect of the item flying from
    /// output slot to its coresponding inventory tab.
    /// </summary>
    private IEnumerator AnimateRetrieveWithVFX(Item item, int count)
    {
        if (itemFlyVFXPrefab == null) yield break;

        // 1. Find the animation's end point (the inventory tab)
        Transform endTransform = null;
        foreach (var tab in tabsManager.tabs)
        {
            if (tab.category == item.category) { endTransform = tab.transform; break; }
        }

        // if no matched category, default to the first tab
        if (endTransform == null && tabsManager.tabs.Length > 0)
        {
            endTransform = tabsManager.tabs[0].transform;
        }

        if (endTransform != null)
        {
            // 2. Get a particle system instance from the object pool
            GameObject vfxGameObject = GameObjectPoolManager.Instance.GetFromPool(itemFlyVFXPrefab, outputSlot.transform.position, Quaternion.identity);
            ParticleSystem vfxInstance = vfxGameObject.GetComponent<ParticleSystem>();

            // 3. Configure the effect's transform
            vfxInstance.transform.SetParent(animationParent);
            vfxInstance.transform.localScale = Vector3.one;

            // 4. Change the particle's sprite to match the crafted item's icon.
            var textureSheetAnimation = vfxInstance.textureSheetAnimation;
            textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
            if (textureSheetAnimation.spriteCount > 0) textureSheetAnimation.RemoveSprite(0);
            textureSheetAnimation.AddSprite(item.sprite);

            // 5. Set up particle bursts based on the item count
            int particleCount = Mathf.Min(count, 5);
            float burstInterval = 0.1f;
            var bursts = new ParticleSystem.Burst[particleCount];
            for (int i = 0; i < particleCount; i++)
            {
                bursts[i] = new ParticleSystem.Burst(i * burstInterval, 1);
            }
            vfxInstance.emission.SetBursts(bursts);

            // 6. Get the particle controller script and tell it to play the animation
            ItemParticleController controller = vfxInstance.GetComponent<ItemParticleController>();
            if (controller != null)
            {
                controller.Play(endTransform, itemFlyVFXPrefab);
            }
        }
    }

    // Refresh the visual of crafting panel
    public void UpdateAllSlotsUI()
    {
        var managerSlots = CraftingManager.instance.CraftingSlots;

        for (int i = 0; i < inputSlots.Count; i++)
        {
            inputSlots[i].UpdateSlot(managerSlots[i]);
        }

        bool isPendingRetrieval = (_currentState == CraftingState.PendingRetrieval);

        if (isPendingRetrieval)
        {
            // fully opaque
            outputSlot.UpdateSlot(CraftingManager.instance.CraftedItemSlot);
            outputSlot.SetPreviewState(true);
        }
        else if (CraftingManager.instance.MatchedRecipe != null)
        {
            // semi-transparent
            var result = CraftingManager.instance.MatchedRecipe.result;
            outputSlot.UpdateSlot(new InventorySlot(result.item) { count = result.count });
            outputSlot.SetPreviewState(false);

            int maxCraftAmount = CraftingManager.instance.CalculateMaxCraftableAmount();
            if (maxCraftAmount > 1)
            {
                UpdatePromptText("Hold(J)");
            }
            else
            {
                UpdatePromptText("Craft(J)");
            }
        }
        else
        {
            outputSlot.UpdateSlot(null);
        }

        // After updating, rebuild the navigatable slots list.
        InitializeNavigatableSlots();
    }

    private void UpdatePromptText(string text)
    {
        bool hasText = !string.IsNullOrEmpty(text);
        if (hasText)
        {
            promptText.text = text;
        }
    }
}
