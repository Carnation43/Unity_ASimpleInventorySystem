using InstanceResetToDefault;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("Core UI References")]
    [SerializeField] private GameObject _canvasObj;
    [SerializeField] private GridLayoutGroup _group;

    [Header("Dependencies (Drag Components Here)")]
    [SerializeField] private InventoryView _inventoryView;
    [SerializeField] private MenuStateManager _stateManager;
    [SerializeField] private InventoryNavigationHandler _navigationHandler;
    [SerializeField] private TabsManager _tabsManager;
    [SerializeField] private MenuAnimator _menuAnimator;
    [SerializeField] private LevelUpView _levelUpView;

    public bool IsMenuOpen { get; private set; }
    private bool _isSwitchingTabs = false;
    private bool _isOpeningMenu = false;
    private bool _isInputLocked = false;
    private Coroutine _toggleMenuCoroutine;

    // Define current focus type
    public enum MenuFocus { Inventory, LevelUp }
    public MenuFocus currentFocus = MenuFocus.Inventory;

    private void Awake()
    {
        instance = this;

        // Subscribe to item addition, deletion, and selection events
        InventoryManager.instance.OnItemAdded += HandleInventoryDataChanged;
        InventoryManager.instance.OnItemRemoved += HandleInventoryDataChanged;
        InventoryManager.instance.OnInventoryUpdated += HandleInventoryDataChanged;
        InventoryManager.instance.OnItemConsumed += HandleItemConsumed;
        ItemsIconAnimationController.OnItemSelected += _stateManager.OnItemSelected;

        _navigationHandler.Initialize(_stateManager, _inventoryView, _group);

        _canvasObj.SetActive(false);
        IsMenuOpen = false;
    }

    private void OnEnable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnToggleMenu += HandleToggleMenu;
            inputChannel.OnGlobalInputLock += HandleInputLock;
            inputChannel.OnNavigate += HandleNavigation;
        }
    }

    private void OnDisable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnToggleMenu -= HandleToggleMenu;
            inputChannel.OnGlobalInputLock -= HandleInputLock;
            inputChannel.OnNavigate -= HandleNavigation;
        }
    }

    private void HandleItemConsumed(InventorySlot slot, bool isDepleted, int consumedSlotIndex)
    {
        // Situation 1: When the item is not depleted
        if (!isDepleted)
        {
            if (_stateManager.LastItemSelected != null)
            {
                var rect = _stateManager.LastItemSelected.GetComponent<RectTransform>();
                TooltipViewController.instance.ShowTooltip(slot, rect);
            }
            return;
        }

        // Situation 2: When the item stack is exhausted, let the inventoryView decide how to handle it.
        // There are three cases:
        // First, when there is only one consumable item.
        // Second, when the consumable item is in the last position.
        // Third, when the consumable item is not in the last position.
        _inventoryView.SelectNewItemAfterConsume(_stateManager.LastSelectedIndex);
    }


    private void HandleInputLock(bool isLocked)
    {
        _isInputLocked = isLocked;
    }

    private void HandleToggleMenu(InputAction.CallbackContext obj)
    {
        if (_isInputLocked) return;

        ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (_toggleMenuCoroutine != null) return;

        IsMenuOpen = !IsMenuOpen;

        if (IsMenuOpen)
        {
            _toggleMenuCoroutine = StartCoroutine(OpenMenuCoroutine());
        }
        else
        {
            _toggleMenuCoroutine = StartCoroutine(CloseMenuCoroutine());
        }

    }
    private IEnumerator OpenMenuCoroutine()
    {
        _isOpeningMenu = true;

        _canvasObj.SetActive(true);
        currentFocus = MenuFocus.Inventory;
        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.SetNavigationMode(false);
        }
        SingletonResetManager.Instance.ResetAllSingletons();
        _stateManager.ClearSelection();
        HandleInventoryDataChanged();

        if (_inventoryView != null)
        {
            yield return _inventoryView.StartCoroutine(_inventoryView.SelectFirstItemAfterDelay(true));
        }

        if (_menuAnimator != null)
        {
            yield return _menuAnimator.PlayOpenAnimation();
        }

        if (UserInput.instance != null)
        {
            UserInput.instance.SwitchActionMap("UI_Inventory");
        }
        _isOpeningMenu = false;

        _toggleMenuCoroutine = null;
    }

    private IEnumerator CloseMenuCoroutine()
    {
        if (_stateManager.LastItemSelected != null)
        {
            var iconController = _stateManager.LastItemSelected.GetComponent<BaseIconAnimationController>();
            if (iconController != null) iconController.OnDeselect(new BaseEventData(EventSystem.current));
        }
        EventSystem.current.SetSelectedGameObject(null);
        _isSwitchingTabs = false;

        if (UserInput.instance != null)
        {
            UserInput.instance.SwitchActionMap("Player");
        }
        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.SetNavigationMode(true);
        }
        if (_menuAnimator != null)
        {
            yield return _menuAnimator.PlayCloseAnimation();
        }

        _canvasObj.SetActive(false);

        _toggleMenuCoroutine = null;
    }

        // Filter the inventory according to item categories
    public void ChangeFilter(int id, bool resetSelection)
    {
        var items = InventoryManager.instance.inventory;
        if (id != 0)
        {
            var category = (ItemCategory)(id - 1);
            items = InventoryManager.instance.inventory.FindAll(x => x.item.category == category);
        }

        int direction = (_isSwitchingTabs && !_isOpeningMenu) ? _tabsManager.navigationDirection : 0;
        _inventoryView.AnimateAndRefresh(items, resetSelection, direction);
        //_inventoryView.RefreshInventoryGrid(items, resetSelection); // replace it with new method
    }

    /// <summary>
    /// This method is called by an event from TabsManager when a tab is switched.
    /// It sets the flag to true before calling ChangeFilter to ensure animation plays.
    /// </summary>
    public void OnTabSwitched(int tabId)
    {
        if (TooltipViewController.instance != null)
        {
            TooltipViewController.instance.HideTooltip(); // hide tooltip when change the tab
        }
        _isSwitchingTabs = true;
        ChangeFilter(tabId, true);
        _isSwitchingTabs = false;
    }

    private void HandleInventoryDataChanged()
    {
        if (!IsMenuOpen) return;
        if (_tabsManager != null) ChangeFilter(_tabsManager.currentTabIndex, false);
    }

    private void OnDestroy()
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.OnItemAdded -= HandleInventoryDataChanged;
            InventoryManager.instance.OnItemRemoved -= HandleInventoryDataChanged;
            InventoryManager.instance.OnInventoryUpdated -= HandleInventoryDataChanged;
            InventoryManager.instance.OnItemConsumed -= HandleItemConsumed;
        }
        ItemsIconAnimationController.OnItemSelected -= _stateManager.OnItemSelected;
    }

    private void HandleNavigation(InputAction.CallbackContext context)
    {
        if (!IsMenuOpen || _isInputLocked) return;

        Vector2 move = context.ReadValue<Vector2>();

        if (context.performed)
        {
            if (currentFocus == MenuFocus.Inventory)
            {
                // When moving left in the first column of the inventory,
                // switch the focus to the upgrade panel
                var gridLayoutSize = GridLayoutGroupHelper.Size(_group);
                bool isFirstColumn = _stateManager.LastItemSelected != null && gridLayoutSize.x > 0 && _stateManager.LastSelectedIndex % gridLayoutSize.x == 0;

                if (move.x < -0.5f && isFirstColumn)
                {
                    SwitchFocusTo(MenuFocus.LevelUp);
                    _navigationHandler.StopContinuousNavigation();
                    return;
                }
            }
            else if (currentFocus == MenuFocus.LevelUp)
            {
                // When moving right in the upgrade panel, switch the focus to the inventory
                if (move.x > 0.5f)
                {
                    SwitchFocusTo(MenuFocus.Inventory);
                    return;
                }
            }
        }
        if (currentFocus == MenuFocus.Inventory)
        {
            if (context.performed)
            {
                _navigationHandler.MoveOneStep(move);
                _navigationHandler.StartContinuousNavigation(move);
            }
        
            else if (context.canceled)
            {
                _navigationHandler.StopContinuousNavigation();
            }
        }
    }

    private void SwitchFocusTo(MenuFocus newFocus)
    {
        if (currentFocus == newFocus) return;

        currentFocus = newFocus;

        if (newFocus == MenuFocus.LevelUp)
        {
            // lose focus
            if (_stateManager.LastItemSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            // TooltipViewController.instance.HideTooltip();

            // levelpanel takes focus
            _levelUpView.TakeFocus();
        }
        else // newFocus == MenuFocus.Inventory
        {
            // levelpanel loses focus
            _levelUpView.LoseFocus();

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
    }
}