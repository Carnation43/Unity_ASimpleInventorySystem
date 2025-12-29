using InstanceResetToDefault;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// This script specifically manages the logic within the inventory page
/// handling focus between the Item Grid, LevelUp Panel, and Crafting Panel.
/// </summary>
public class InventoryController : MonoBehaviour, IResettableUI, IMenuPage
{
    public static InventoryController instance;

    // InventoryEventChannel: The Listener
    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;
    [SerializeField] private InventoryEventChannel inventoryChannel;

    [Header("Core UI References")]
    [SerializeField] private GridLayoutGroup _group;

    [Header("Dependencies (Drag Components Here)")]
    [SerializeField] private TooltipViewController _tootip;
    [SerializeField] private InventoryView _inventoryView;
    [SerializeField] private SelectionStateManager _stateManager;
    [SerializeField] private GridNavigationHandler _navigationHandler;
    [SerializeField] private TabsManager _tabsManager;
    [SerializeField] private LevelUpView _levelUpView;
    [SerializeField] private CraftingView _craftingView;

    // used to control the interactability of different sub-panels
    [Header("Panel Canvas group")]
    [SerializeField] private CanvasGroup _inventoryPanelCanvasGroup;
    [SerializeField] private CanvasGroup _levelUpPanelCanvasGroup;
    [SerializeField] private CanvasGroup _craftingPanelCanvasGroup;

    private bool _isSwitchingTabs = false;

    // Define current focus type
    public enum MenuFocus { Inventory, LevelUp, Crafting }
    public MenuFocus currentFocus = MenuFocus.Inventory;

    // Interface
    public GameObject PageGameObject => this.gameObject;

    private void Awake()
    {
        instance = this;

        ItemsIconAnimationController.OnItemSelected += _stateManager.OnItemSelected;

        _navigationHandler.Initialize(_stateManager, _inventoryView);
    }

    private void OnDestroy()
    {
        ItemsIconAnimationController.OnItemSelected -= _stateManager.OnItemSelected;
    }

    private void OnEnable()
    {
        SubscribeToInputs();

        if (inventoryChannel != null)
        {
            inventoryChannel.OnInventoryUpdated += HandleInventoryDataChanged;
            inventoryChannel.OnItemConsumed += HandleItemConsumed;
        }
        else
        {
            Debug.LogError("[InventoryController] Inventory Event Channel is not assigned. ");
        }
    }

    private void OnDisable()
    {
        // Ensure inputs are unsubscribed when disabled
        UnsubscribeFromInputs();

        // Unsubscribe from Inventory Channel Events
        if (inventoryChannel != null)
        {
            inventoryChannel.OnInventoryUpdated -= HandleInventoryDataChanged;
            inventoryChannel.OnItemConsumed -= HandleItemConsumed;
        }
    }

    // Called by MenuController when switching to this page
    public void OpenPage()
    {
        gameObject.SetActive(true);

        // Logic from MenuController.OpenMenuCoroutine()
        currentFocus = MenuFocus.Inventory;

        // Disable Unity's default navigation to use our custom GridNavigationHandler.
        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.SetNavigationMode(false);
        }

        // Reset states
        ResetUI();

        // Update UI
        HandleInventoryDataChanged();
        _craftingView.UpdateAllSlotsUI();

        // Select first item
        if (_inventoryView != null)
        {
            StartCoroutine(_inventoryView.SelectFirstItemAfterDelay(true));
        }
    }

    // Called by MenuController when leaving this page.
    public void ClosePage()
    {
        // Auto-retrieve items from crafting slots when closing
        if (CraftingManager.instance != null)
        {
            CraftingManager.instance.RetrieveAllItemsToInventory();
        }

        // Logic from MenuController.CloseMenuCoroutine()
        if (_stateManager.LastItemSelected != null)
        {
            var iconController = _stateManager.LastItemSelected.GetComponent<BaseIconAnimationController>();
            if (iconController != null) iconController.OnDeselect(new BaseEventData(EventSystem.current));
        }
        EventSystem.current.SetSelectedGameObject(null);
        _isSwitchingTabs = false;

        gameObject.SetActive(false);
    }

    public void ResetPage()
    {
        ResetUI();
    }

    // Input subscription management
    private void SubscribeToInputs()
    {
        if (inputChannel != null)
        {
            inputChannel.OnNavigate += HandleNavigation;
        }
    }

    // Input unsubscription management
    private void UnsubscribeFromInputs()
    {
        if (inputChannel != null)
        {
            inputChannel.OnNavigate -= HandleNavigation;
        }
    }

    private void HandleItemConsumed(InventorySlot slot, bool isDepleted, int consumedSlotIndex)
    {
        // Situation 1: When the item is not depleted
        if (!isDepleted)
        {
            return;
        }

        // Situation 2: When the item stack is exhausted, let the inventoryView decide how to handle it.
        // There are three cases:
        // First, when there is only one consumable item.
        // Second, when the consumable item is in the last position.
        // Third, when the consumable item is not in the last position.
        _inventoryView.SelectNewItemAfterConsume(_stateManager.LastSelectedIndex);
    }

    // Filter the inventory according to item categories
    public void ChangeFilter(ItemCategory category, bool resetSelection)
    {
        List<InventorySlot> items;

        if (category == ItemCategory.All)
        {
            items = InventoryManager.instance.inventory;
        }
        else
        {
            items = InventoryManager.instance.inventory.FindAll(x => x.item.category == category);
        }

        // _isOpeningMenu check is no longer needed here
        int direction = (_isSwitchingTabs) ? _tabsManager.navigationDirection : 0;
        _inventoryView.AnimateAndRefresh(items, resetSelection, direction);
        //_inventoryView.RefreshInventoryGrid(items, resetSelection); // replace it with new method
    }

    /// <summary>
    /// Unity inspector configurable function
    /// This method is called by an event from TabsManager when a tab is switched.
    /// It sets the flag to true before calling ChangeFilter to ensure animation plays.
    /// </summary>
    public void OnTabSwitched()
    {
        if (_tootip != null) _tootip.HideTooltip();

        int currentIndex = _tabsManager.currentTabIndex;
        if (currentIndex < 0 || currentIndex >= _tabsManager.tabs.Length)
        {
            Debug.LogError($"[InventoryController] currentTabIndex {currentIndex} invalid¡£");
            return;
        }

        ItemCategory filterCategory = _tabsManager.tabs[currentIndex].category;

        _isSwitchingTabs = true;
        ChangeFilter(filterCategory, true);
        _isSwitchingTabs = false;
    }

    private void HandleInventoryDataChanged()
    {
        // if (!IsMenuOpen) return; // Replaced with check below
        if (!gameObject.activeInHierarchy) return;
        if (_tabsManager != null)
        {
            int currentIndex = _tabsManager.currentTabIndex;
            if (currentIndex < 0 || currentIndex >= _tabsManager.tabs.Length) return; 

            ItemCategory currentCategory = _tabsManager.tabs[currentIndex].category;

            ChangeFilter(currentCategory, false);
        }
    }

    // Complex navigation logic to switch focus between panels based on input.
    private void HandleNavigation(InputAction.CallbackContext context)
    {
        Debug.Log($"<color=cyan>InventoryController: Focus: {currentFocus}</color>");
        // if (!IsMenuOpen || _isInputLocked || UserInput.IsRadialMenuHeldDown) return; // Replaced with check below
        if (!gameObject.activeInHierarchy || (inputChannel != null && inputChannel.IsInputLocked) || UserInput.IsRadialMenuHeldDown) return;

        Vector2 move = context.ReadValue<Vector2>();

        if (context.performed)
        {
            switch (currentFocus)
            {
                case MenuFocus.Inventory:
                    // When moving left in the first column of the inventory,
                    // switch the focus to the upgrade panel
                    var gridLayoutSize = GridLayoutGroupHelper.Size(_group);
                    bool isFirstColumn = _stateManager.LastItemSelected != null && gridLayoutSize.x > 0 && _stateManager.LastSelectedIndex % gridLayoutSize.x == 0;

                    if (move.x < -0.5f && isFirstColumn)
                    {
                        SwitchFocusTo(MenuFocus.LevelUp);
                        _navigationHandler.StopContinuousNavigation();
                    }
                    else if (move.y < -0.5f && _navigationHandler.IsOnLastRow())
                    {
                        SwitchFocusTo(MenuFocus.Crafting);
                        _navigationHandler.StopContinuousNavigation();
                    }
                    else
                    {
                        _navigationHandler.MoveOneStep(move);
                        _navigationHandler.StartContinuousNavigation(move);
                    }
                    break;

                case MenuFocus.LevelUp:
                    // When moving right in the upgrade panel, switch the focus to the inventory
                    if (move.x > 0.5f)
                    {
                        SwitchFocusTo(MenuFocus.Inventory);
                    }
                    break;

                case MenuFocus.Crafting:
                    var slots = _craftingView.GetNavigatableSlots();
                    var currentSelected = EventSystem.current.currentSelectedGameObject;
                    int currentIndex = slots.IndexOf(currentSelected);

                    int direction = 0;
                    if (move.x > 0.5f) direction = 1;
                    else if (move.x < -0.5f) direction = -1;

                    if (direction != 0 && currentIndex != -1)
                    {
                        int newIndex = (currentIndex + direction + slots.Count) % slots.Count;
                        EventSystem.current.SetSelectedGameObject(slots[newIndex]);
                    }

                    if (move.y > 0.5f)
                    {
                        SwitchFocusTo(MenuFocus.Inventory);
                    }
                    break;
            }
        }

        if (currentFocus == MenuFocus.Inventory && context.canceled)
        {
            _navigationHandler.StopContinuousNavigation();
        }
    }

    private void SwitchFocusTo(MenuFocus newFocus)
    {
        if (currentFocus == newFocus) return;

        // clean up status of the previous focus
        switch (currentFocus)
        {
            case MenuFocus.Inventory:
                if (_stateManager.LastItemSelected != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
                _tootip.HideTooltip();
                break;
            case MenuFocus.LevelUp:
                _levelUpView.LoseFocus();
                break;
            case MenuFocus.Crafting:
                EventSystem.current.SetSelectedGameObject(null);
                break;
        }

        currentFocus = newFocus;

        // inactive canvasgroup
        SetCanvasGroupState(_inventoryPanelCanvasGroup, false);
        SetCanvasGroupState(_levelUpPanelCanvasGroup, false);
        SetCanvasGroupState(_craftingPanelCanvasGroup, false);

        if (newFocus == MenuFocus.LevelUp)
        {
            SetCanvasGroupState(_levelUpPanelCanvasGroup, true);
            _levelUpView.TakeFocus();
        }
        else if (newFocus == MenuFocus.Inventory)
        {
            SetCanvasGroupState(_inventoryPanelCanvasGroup, true);

            // inventoryPanel takse focus
            if (_stateManager.LastItemSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(_stateManager.LastItemSelected);
            }
            else
            {
                StartCoroutine(_inventoryView.SelectFirstItemAfterDelay(true));
            }
        }
        else if (newFocus == MenuFocus.Crafting)
        {
            SetCanvasGroupState(_craftingPanelCanvasGroup, true);
            EventSystem.current.SetSelectedGameObject(_craftingView.GetNavigatableSlots().FirstOrDefault());
        }
    }

    // IResettableUI implementation
    public void ResetUI()
    {
        if (_tabsManager != null) _tabsManager.ResetUI();
        if (_levelUpView != null) _levelUpView.ResetUI();
        if (_tootip != null) _tootip.ResetUI();

        if (_stateManager != null) _stateManager.ClearSelection();

        currentFocus = MenuFocus.Inventory;
        _isSwitchingTabs = false;
        ResetPanelVisibilities();
        if (_navigationHandler != null) _navigationHandler.StopContinuousNavigation();
    }

    private void ResetPanelVisibilities()
    {
        SetCanvasGroupState(_inventoryPanelCanvasGroup, true);  
        SetCanvasGroupState(_levelUpPanelCanvasGroup, false);
        SetCanvasGroupState(_craftingPanelCanvasGroup, false);
    }

    private void SetCanvasGroupState(CanvasGroup cg, bool isActive)
    {
        if (cg == null) return;
        cg.interactable = isActive;
        cg.blocksRaycasts = isActive;
    }
    #region Change Menu
    /// <summary>
    /// Switch to craft menu
    /// </summary>
    public void OpenCraftMenu()
    {
        Debug.Log("<color=cyan>Open the craft menu</color>");
    }
    #endregion
}