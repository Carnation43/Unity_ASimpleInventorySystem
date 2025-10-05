using InstanceResetToDefault;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("Core UI References")]
    [SerializeField] private GameObject _canvasObj;
    [SerializeField] private GridLayoutGroup _group;

    [Header("Dependencies (Drag Components Here)")]
    [SerializeField] private InventoryView _viewController;
    [SerializeField] private MenuStateManager _stateManager;
    [SerializeField] private InventoryNavigationHandler _navigationHandler;
    [SerializeField] private TabsManager _tabsManager;

    public bool IsMenuOpen { get; private set; }
    private bool _isSwitchingTabs = false;
    private bool _isOpeningMenu = false;

    private void Awake()
    {
        // Subscribe to item addition, deletion, and selection events
        InventoryManager.instance.OnItemAdded += HandleInventoryDataChanged;
        InventoryManager.instance.OnItemRemoved += HandleInventoryDataChanged;
        InventoryManager.instance.OnInventoryUpdated += HandleInventoryDataChanged;
        ItemsIconAnimationController.OnItemSelected += _stateManager.OnItemSelected;

        _navigationHandler.Initialize(_stateManager, _viewController, _group);

        _canvasObj.SetActive(false);
        IsMenuOpen = false;
    }

    private void OnEnable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnToggleMenu += HandleToggleMenu;
        }
    }

    private void OnDisable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnToggleMenu -= HandleToggleMenu;
        }
    }

    private void Update()
    {
        if (!IsMenuOpen) return;

        _navigationHandler.HandleNavigationInput();
    }

    private void HandleToggleMenu(InputAction.CallbackContext obj)
    {
        ToggleMenu();
    }

    public void ToggleMenu()
    {
        IsMenuOpen = !IsMenuOpen;

        if (IsMenuOpen)
        {
            _isOpeningMenu = true;
            _canvasObj.SetActive(true);
            // default reset
            SingletonResetManager.Instance.ResetAllSingletons();
            _stateManager.ClearSelection();
            HandleInventoryDataChanged();

            if (UserInput.instance != null)
            {
                UserInput.instance.SwitchActionMap("UI_Inventory");
            }
            _isOpeningMenu = false;
        }
        else
        {
            if (_stateManager.LastItemSelected != null)
            {
                var iconController = _stateManager.LastItemSelected.GetComponent<BaseIconAnimationController>();
                if (iconController != null) iconController.OnDeselect(new BaseEventData(EventSystem.current));
            }
            EventSystem.current.SetSelectedGameObject(null);
            _canvasObj.SetActive(false);
            _isSwitchingTabs = false;

            if (UserInput.instance != null)
            {
                UserInput.instance.SwitchActionMap("Player");
            }
        }

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
        _viewController.AnimateAndRefresh(items, resetSelection, direction);
        //_viewController.RefreshInventoryGrid(items, resetSelection); // replace it with new method
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
        }
        ItemsIconAnimationController.OnItemSelected -= _stateManager.OnItemSelected;
    }
}