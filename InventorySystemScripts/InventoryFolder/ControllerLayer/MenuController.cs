using InstanceResetToDefault;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("Broadcasting On")]
    [SerializeField] private MusicEventChannel musicChannel;
    [SerializeField] private AudioCueEventChannel uiAudioChannel;

    [Header("SFX References")]
    [SerializeField] private AudioCueSO onToggleMenuCue;

    [Header("Core UI References")]
    [SerializeField] private GameObject _canvasObj;
    [SerializeField] private GridLayoutGroup _group;
    [SerializeField] private GameObject _backgroundCanvasObj;

    [Header("Dependencies (Drag Components Here)")]
    [SerializeField] private InventoryView _inventoryView;
    [SerializeField] private MenuStateManager _stateManager;
    [SerializeField] private InventoryNavigationHandler _navigationHandler;
    [SerializeField] private TabsManager _tabsManager;
    [SerializeField] private MenuAnimator _menuAnimator;
    [SerializeField] private LevelUpView _levelUpView;
    [SerializeField] private CraftingView _craftingView;

    [Header("Panel Canvas group")]
    [SerializeField] private CanvasGroup _inventoryPanelCanvasGroup;
    [SerializeField] private CanvasGroup _levelUpPanelCanvasGroup;
    [SerializeField] private CanvasGroup _craftingPanelCanvasGroup;

    public bool IsMenuOpen { get; private set; }
    private bool _isSwitchingTabs = false;
    private bool _isOpeningMenu = false;
    private bool _isInputLocked = false;
    private Coroutine _toggleMenuCoroutine;

    // Define current focus type
    public enum MenuFocus { Inventory, LevelUp, Crafting }
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
        _backgroundCanvasObj.SetActive(false);
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
        Debug.Log(gameObject.name + " received lock event: " + isLocked);
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
        yield return null;

        uiAudioChannel?.RaiseEventWithPitch(onToggleMenuCue, 1f);
        musicChannel?.RaiseApplyEffectEvent();

        _isOpeningMenu = true;

        _canvasObj.SetActive(true);
        currentFocus = MenuFocus.Inventory;

        if (_levelUpPanelCanvasGroup != null)
        {
            _levelUpPanelCanvasGroup.interactable = false;
            _levelUpPanelCanvasGroup.blocksRaycasts = false;
        }
        if (_inventoryPanelCanvasGroup != null)
        {
            _inventoryPanelCanvasGroup.interactable = true;
            _inventoryPanelCanvasGroup.blocksRaycasts = true;
        }

        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.SetNavigationMode(false);
        }
        SingletonResetManager.Instance.ResetAllSingletons();
        _stateManager.ClearSelection();
        HandleInventoryDataChanged();
        _craftingView.UpdateAllSlotsUI();

        if (_inventoryView != null)
        {
            yield return _inventoryView.StartCoroutine(_inventoryView.SelectFirstItemAfterDelay(true));
        }

        if (_menuAnimator != null)
        {
            yield return _menuAnimator.PlayOpenAnimation();
        }

        _backgroundCanvasObj.SetActive(true);

        if (UserInput.instance != null)
        {
            UserInput.instance.SwitchActionMap("UI_Inventory");
        }
        _isOpeningMenu = false;

        _toggleMenuCoroutine = null;
    }

    private IEnumerator CloseMenuCoroutine()
    {
        _backgroundCanvasObj.SetActive(false);

        uiAudioChannel?.RaiseEventWithPitch(onToggleMenuCue, 1.2f);
        musicChannel?.RaiseRemoveEffectEvent();

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
        _backgroundCanvasObj.SetActive(false);
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
        if (!IsMenuOpen || _isInputLocked || UserInput.IsRadialMenuHeldDown) return;
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

        // clean status
        switch (currentFocus)
        {
            case MenuFocus.Inventory:
                if (_stateManager.LastItemSelected != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
                TooltipViewController.instance.HideTooltip();
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
        if (_inventoryPanelCanvasGroup != null)
        {
            _inventoryPanelCanvasGroup.interactable = false;
            _inventoryPanelCanvasGroup.blocksRaycasts = false;
        }
        if (_levelUpPanelCanvasGroup != null)
        {
            _levelUpPanelCanvasGroup.interactable = false;
            _levelUpPanelCanvasGroup.blocksRaycasts = false;
        }
        if (_craftingPanelCanvasGroup != null)
        {
            _craftingPanelCanvasGroup.interactable = false;
            _craftingPanelCanvasGroup.blocksRaycasts = false;
        }

        if (newFocus == MenuFocus.LevelUp)
        {
            _levelUpPanelCanvasGroup.interactable = true;   
            _levelUpPanelCanvasGroup.blocksRaycasts = true; 
            _levelUpView.TakeFocus();
        }
        else if (newFocus == MenuFocus.Inventory)
        {
            _inventoryPanelCanvasGroup.interactable = true;  
            _inventoryPanelCanvasGroup.blocksRaycasts = true; 
            
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
            if (_craftingPanelCanvasGroup != null)
            {
                _craftingPanelCanvasGroup.interactable = true;
                _craftingPanelCanvasGroup.blocksRaycasts = true;
            }
            EventSystem.current.SetSelectedGameObject(_craftingView.GetNavigatableSlots().FirstOrDefault());
        }
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