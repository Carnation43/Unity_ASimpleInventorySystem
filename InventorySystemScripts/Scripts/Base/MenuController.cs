using InstanceResetToDefault;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// It acts as a heigh-level "Page Manager" that toggles the main menu state
/// and switches between different sub-pages (Inventory, RecipeBook, etc.) via the IMenuPage interface.
/// </summary>
public class MenuController : MonoBehaviour, IResettableUI
{
    public static MenuController instance;
    public event Action OnSideMenuExit;

    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("Broadcasting On")]
    [SerializeField] private MusicEventChannel musicChannel;
    [SerializeField] private AudioCueEventChannel uiAudioChannel;

    [Header("SFX References")]
    [SerializeField] private AudioCueSO onToggleMenuCue;

    [Header("Core UI References")]
    [SerializeField] private GameObject _canvasObj;
    [SerializeField] private GameObject _backgroundCanvasObj;
    [SerializeField] private Image _backgroundImage;

    [Header("Dependencies (Drag Components Here)")]
    [SerializeField] private MenuAnimator _menuAnimator;

    // This allows the MenuController to manage any number of pages
    // without knowing their specific implementation.
    [Header("Page Controllers")]
    [SerializeField] private List<GameObject> _pageGameObjects;

    // Reference to the SideMenu used in the "Resting" state.
    [Header("Rest System")]
    [SerializeField] private SideMenuController _sideMenuController;

    private List<IMenuPage> _allPages = new List<IMenuPage>();
    private IMenuPage _currentPage;

    public bool IsMenuOpen { get; private set; }
    private bool _isInSideMenu = false;
    private Coroutine _toggleMenuCoroutine;

    #region Unity Lifecycle
    private void Awake()
    {
        if (instance == null) instance = this;

        // Initialize pages by finding the IMenuPage interface on the referenced GameObject.
        _allPages = new List<IMenuPage>();
        foreach (var go in _pageGameObjects)
        {
            if (go != null && go.TryGetComponent(out IMenuPage page))
            {
                _allPages.Add(page);
            }
        }
        IsMenuOpen = false;
    }

    private void Start()
    {
        ResetUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Debug.LogWarning("Force Reset Input Lock£¡");
            if (inputChannel != null) inputChannel.RaiseGlobalInputLockEvent(false);
        }
    }

    private void OnEnable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnToggleMenu += HandleToggleMenu;

            inputChannel.OnCancel += HandleCancelInput;
        }

        if (_sideMenuController != null)
        {
            _sideMenuController.OnExitRequested += HandleSideMenuExitRequest;
            _sideMenuController.OnPageRequested += HandleSideMenuPageRequest;
        }
    }

    private void OnDisable()
    {
        ResetUI();

        if (inputChannel != null)
        {
            inputChannel.OnToggleMenu -= HandleToggleMenu;

            inputChannel.OnCancel -= HandleCancelInput;
        }

        if (_sideMenuController != null)
        {
            _sideMenuController.OnExitRequested -= HandleSideMenuExitRequest;
            _sideMenuController.OnPageRequested -= HandleSideMenuPageRequest;
        }
    }

    #endregion

    #region Private Methods
    private void ToggleMenu()
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

    // Generic method to switch active pages.
    // It calls ClosePage() on the old page and OpenPage() on the new one.
    private void SwitchToPage(IMenuPage newPage)
    {
        if (newPage == null || newPage == _currentPage) return;

        if (_currentPage != null)
        {
            _currentPage.ClosePage();
        }

        _currentPage = newPage;
        _currentPage.OpenPage();
    }

    private void FinalizeMenuClose()
    {
        if (_canvasObj != null) _canvasObj.SetActive(false);
        if (_backgroundCanvasObj != null) _backgroundCanvasObj.SetActive(false);
        if (_backgroundImage != null) _backgroundImage.enabled = false;

        ResetAllPages();

        IsMenuOpen = false;
        _isInSideMenu = false;

        // Switch back to Player control map when menu closes.
        if (UserInput.instance != null)
        {
            UserInput.instance.SwitchActionMap("Player");
        }
        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.SetNavigationMode(true);
        }
    }

    private void ResetAllPages()
    {
        foreach (var page in _allPages)
        {
            page.ResetPage();
            page.PageGameObject.SetActive(false);
        }
    }
    #endregion

    // 1. Rest Mode Interface (Called by RestSystemController)

    /// <summary>
    /// Opens the rest menu
    /// </summary>
    public void OpenRestMenu()
    {
        // 1. Active Canvas
        _canvasObj.SetActive(true);
        _backgroundImage.enabled = false;   // disabled background to see the game scene.
        _menuAnimator.SetAlphaInstantly(1f);

        // 2. Ensure SideMenu is active and shown
        if (_sideMenuController != null)
        {
            _sideMenuController.gameObject.SetActive(true);

            // Hide all other pages, keep only SideMenu
            foreach (var page in _allPages)
            {
                if (page.PageGameObject != _sideMenuController.gameObject)
                {
                    page.PageGameObject.SetActive(false);
                }
            }

            // Call logic to show SideMenu
            _sideMenuController.OpenPage();
        }

        // 3. switch input to UI module
        if (UserInput.instance != null)
        {
            UserInput.instance.SwitchActionMap("UI_Inventory");
        }

        IsMenuOpen = true;
        _isInSideMenu = true;
    }

    #region Input Handlers

    private void HandleToggleMenu(InputAction.CallbackContext obj)
    {
        // Prevent toggling via 'M' key when we are in Rest Mode.
        if ((inputChannel != null && inputChannel.IsInputLocked) || _isInSideMenu) return;

        ToggleMenu();
    }

    /// <summary>
    /// Called when SideMenu requests an entry.
    /// </summary>
    /// <param name="obj"></param>
    private void HandleSideMenuPageRequest(SideMenuType type)
    {
        IMenuPage targetPage = null;

        switch (type)
        {
            case SideMenuType.Inventory:
                targetPage = _allPages.Find(p => p is InventoryController);
                break;

            case SideMenuType.Recipe:
                targetPage = _allPages.Find(p => p is RecipeBookController);
                break;
        }

        if (targetPage != null)
        {
            StartCoroutine(FromRestToSubPage(targetPage));
        }
    }

    /// <summary>
    /// Called when SideMenu requests an exit.
    /// </summary>
    private void HandleSideMenuExitRequest()
    {
        OnSideMenuExit?.Invoke();
    }

    /// <summary>
    /// Called when return to SideMenu
    /// </summary>
    private void HandleCancelInput(InputAction.CallbackContext context)
    {
        if (!IsMenuOpen) return;
        if ((inputChannel != null && inputChannel.IsInputLocked)) return;

        if (_isInSideMenu)
        {
            // If we are in a sub-page, go back to SideMenu
            if (_currentPage != _sideMenuController)
            {
                StartCoroutine(ReturnToSideMenuCoroutine());
            }
        }
        else
        {
            // Standard behavior: Close menu
            ToggleMenu();
        }
    }
    #endregion

    #region Coroutines

    // Transition from Sub-Page back to SideMenu
    private IEnumerator ReturnToSideMenuCoroutine()
    {
        Debug.Log("[MenuController] resume Side Menu");
        // 1. Lock input
        if (inputChannel != null) inputChannel.RaiseGlobalInputLockEvent(true);

        // 2. Close current sub-page
        if (_currentPage != null)
        {
            _currentPage.ClosePage();
            Debug.Log($"[MenuController] _currentPage: {_currentPage.PageGameObject.name} has been closed");
        }

        // 3. Reset visual
        if (_backgroundImage != null) _backgroundImage.enabled = false;
        if (_backgroundCanvasObj != null) _backgroundCanvasObj.SetActive(false); 

        // 4. Wait for close animation
        yield return new WaitForSecondsRealtime(0.2f);

        // 5. Open side menu
        if (_sideMenuController != null)
        {
            _sideMenuController.gameObject.SetActive(true);
            _sideMenuController.OpenPage();

            // Update current page record
            _currentPage = _sideMenuController;
        }

        // 6. Unlock input
        if (inputChannel != null) inputChannel.RaiseGlobalInputLockEvent(false);
    }

    private IEnumerator OpenMenuCoroutine()
    {
        yield return null;

        uiAudioChannel?.RaiseEventWithPitch(onToggleMenuCue, 1f);
        musicChannel?.RaiseApplyEffectEvent();

        _canvasObj.SetActive(true);
        _backgroundImage.enabled = true;

        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.SetNavigationMode(false);
        }

        // Default open the first page (Inventory)
        SwitchToPage(_allPages[0]);

        if (UserInput.instance != null)
        {
            UserInput.instance.SwitchActionMap("UI_Inventory");
        }

        if (_menuAnimator != null)
        {
            yield return _menuAnimator.PlayOpenAnimation();
        }

        _toggleMenuCoroutine = null;

        _backgroundCanvasObj.SetActive(true);

    }

    private IEnumerator CloseMenuCoroutine()
    {
        _backgroundCanvasObj.SetActive(false);

        uiAudioChannel?.RaiseEventWithPitch(onToggleMenuCue, 1.2f);
        musicChannel?.RaiseRemoveEffectEvent();

        if (_currentPage != null)
        {
            _currentPage.ClosePage();
            _currentPage = null;
        }

        if (_menuAnimator != null)
        {
            yield return _menuAnimator.PlayCloseAnimation();
        }

        FinalizeMenuClose();
        _toggleMenuCoroutine = null;
    }

    public void ResetUI()
    {
        StopAllCoroutines();
        _toggleMenuCoroutine = null;

        ResetAllPages();

        _currentPage = null;

        if (_menuAnimator != null)
        {
            _menuAnimator.SetAlphaInstantly(0);
        }

        FinalizeMenuClose();

        Debug.Log("[MenuController] UI Reset Complete.");
    }

    private IEnumerator FromRestToSubPage(IMenuPage targetPage)
    {
        if (_sideMenuController != null)
        {
            _sideMenuController.ClosePage();
        }

        yield return new WaitForSecondsRealtime(0.25f);

        if (_sideMenuController != null)
        {
            _sideMenuController.gameObject.SetActive(false);
        }

        uiAudioChannel?.RaiseEventWithPitch(onToggleMenuCue, 1f);
        musicChannel?.RaiseApplyEffectEvent();

        _canvasObj.SetActive(true);
        _backgroundImage.enabled = true;

        SwitchToPage(targetPage);

        if (_menuAnimator != null)
        {
            _menuAnimator.SetAlphaInstantly(0f);
            yield return _menuAnimator.PlayOpenAnimation();
        }

        _backgroundCanvasObj.SetActive(true);
    }
    #endregion

    #region Public Methods
    public void CloseResetUI()
    {
        StopAllCoroutines();

        if (_sideMenuController != null)
        {
            _sideMenuController.ClosePage();
        }

        FinalizeMenuClose();
    }
    #endregion
}