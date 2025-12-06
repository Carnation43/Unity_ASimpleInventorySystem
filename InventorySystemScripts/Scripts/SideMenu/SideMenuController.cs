using UnityEngine;
using System;
using System.Collections.Generic;
using InstanceResetToDefault;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Controller for the Side Menu in the Rest System.
/// Handles input navigation, button selection logic, and communicates actions
/// </summary>
public class SideMenuController : MonoBehaviour, IMenuPage, IResettableUI
{
    // Event triggered when the player wants to close the menu (e.g. clicking Depart)
    public event Action OnExitRequested;

    // Event triggered when the player wants to open the sub pages.
    public event Action<SideMenuType> OnPageRequested;

    [Header("Listening To")]
    [SerializeField] private InputEventChannel _inputChannel;

    [Header("Internal References")]
    [SerializeField] private SideMenuView _view;

    // Interface Implementation
    public GameObject PageGameObject => this.gameObject;

    // record last select
    private GameObject _lastSelectedObj;
    private SideMenuButton _currentBtn;

    private void OnEnable()
    {
        if (_inputChannel != null)
        {
            _inputChannel.OnConfirm += HandleConfirmInput;
            _inputChannel.OnCancel += HandleCancelInput;
        }

        if (_view != null && _view.Buttons != null)
        {
            foreach (var btn in _view.Buttons)
            {
                btn.OnSelected += HandleButtonSelected;
            }
        }
    }

    private void OnDisable()
    {
        if (_inputChannel != null)
        {
            _inputChannel.OnConfirm -= HandleConfirmInput;
            _inputChannel.OnCancel -= HandleCancelInput;
        }

        if (_view != null && _view.Buttons != null)
        {
            foreach (var btn in _view.Buttons)
            {
                btn.OnSelected -= HandleButtonSelected;
            }
        }

        _currentBtn = null;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        if (EventSystem.current == null) return;

        GameObject current = EventSystem.current.currentSelectedGameObject;

        // anything selected
        if (current != null)
        {
            // focus changed
            if (current != _lastSelectedObj)
            {
                _lastSelectedObj = current;

                _currentBtn = current.GetComponent<SideMenuButton>();
            }
        }
        // focus losed
        else
        {
            if (_lastSelectedObj != null && _lastSelectedObj.activeInHierarchy)
            {
                if (UserInput.UIMoveInput != Vector2.zero)
                {
                    EventSystem.current.SetSelectedGameObject(_lastSelectedObj);
                }
            }
        }
    }

    private void Show()
    {
        // 1. Show the visual elements
        _view.SetMenuVisibility(true);

        // 2. Update dynamic data (e.g. Location Name) - Placeholder for now
        _view.UpdateLoactionInfo("Lily of the Valley Slope", "Entrance");

        // 3. Force select the first button so keyboard navigation works immediately
        _view.SelectFirstButton();
    }

    private void Hide()
    {
        _view.SetMenuVisibility(false, 0.25f);

        _currentBtn = null;
    }

    private void OnMenuButtonClicked(SideMenuType type)
    {
        switch (type)
        {
            // Placeholders for other buttons
            case SideMenuType.Inventory:
            case SideMenuType.Recipe:
                OnPageRequested?.Invoke(type);
                break;

            case SideMenuType.FastTravel:
            case SideMenuType.Memories:
            case SideMenuType.Save:
            case SideMenuType.Depart:
                Debug.Log($"[SideMenu] Button '{type}' clicked. Functionality not yet implemented in this test phase.");
                break;
        }
    }

    // --- IMenuPage Interface Methods (Compatibility) ---
    public void OpenPage()
    {
        // active self
        gameObject.SetActive(true);

        // Hand over UI Control to Unity
        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.SetNavigationMode(true);
        }

        Show();
    }
    public void ClosePage() { Hide(); }
    public void ResetPage()
    {
        ResetUI();
    }

    // --- IResettableUI ---
    public void ResetUI()
    {
        if (_view != null)
        {
            _view.ResetUI();
        }
    }

    // --- Events ---

    private void HandleConfirmInput(InputAction.CallbackContext context)
    {
        if (!gameObject.activeInHierarchy) return;

        if (!context.started) return;

        if (_currentBtn != null && EventSystem.current.currentSelectedGameObject == _currentBtn.gameObject)
        {
            // Audio && Animation
            _currentBtn.OnConfirm();
            OnMenuButtonClicked(_currentBtn.menuType);
        }
    }

    private void HandleCancelInput(InputAction.CallbackContext context)
    {
        if (gameObject.activeInHierarchy) OnExitRequested?.Invoke();
    }

    private void HandleButtonSelected(SideMenuButton btn)
    {
        _currentBtn = btn;
    }
}