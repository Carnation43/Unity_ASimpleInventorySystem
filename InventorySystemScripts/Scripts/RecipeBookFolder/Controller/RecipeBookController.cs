using DG.Tweening;
using InstanceResetToDefault;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// The main controller for the Recipe Book page.
/// </summary>
public class RecipeBookController : MonoBehaviour, IResettableUI, IMenuPage
{
    // TODO: 
    public static event Action<Transform, float> OnUnlockChargeStart;
    // TODO:
    public static event Action OnUnlockChargeCancel;
    public static event Action OnUnlockFailed;

    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;
    [SerializeField] private RecipeEventChannel recipeChannel;

    [Header("SFX Broadcasting On")]
    [SerializeField] private AudioCueEventChannel uiAudioChannel;
    [SerializeField] private AudioSource holdAudioSource;
    [SerializeField] private AudioCueSO onRecipeUnlockCue;
    [SerializeField] private AudioCueSO onRecipeUnlockHoldCue;

    [Header("Component Dependencies")]
    [SerializeField] private RecipeBookView _recipeBookView;
    [SerializeField] private GridLayoutGroup _gridLayoutGroup;
    [SerializeField] private RecipeBookTabsManager _tabsManager;
    [SerializeField] private List<RecipeDetailsView> _recipeDetailsView;
    // Reference to the blur logic script
    [SerializeField] private RecipePanelLogic _recipePanelLogic;
    // TODO: View, Tabs

    private SelectionStateManager _stateManager;
    private GridNavigationHandler _navigationHandler;

    // --- Switching tab START ---
    private bool _isSwitchingTabs = false;
    // --- Switching tab END ---

    // --- Unlock Mechanism START ---
    private RecipeStatus _currentSelectedRecipeStatus;
    private RecipeSlotUI _currentSelectedSlotUI;
    private bool _isHoldToUnlockActive = false;
    private Coroutine _cancelAnimationCoroutine;
    private Coroutine _unlockTimerCoroutine;
    private float _currentUnlockDuration;
    // --- Unlock Mechanism END ---

    public GameObject PageGameObject => this.gameObject;

    // --- State Machine START ---
    private enum RecipeBookState
    {
        Navigating,
        HoldingToUnlock
    }
    private RecipeBookState _currentState;
    // --- State Machine END ---

    private void Awake()
    {
        _stateManager = GetComponent<SelectionStateManager>();
        _navigationHandler = GetComponent<GridNavigationHandler>();

        _navigationHandler.Initialize(_stateManager, _recipeBookView);

        if (holdAudioSource != null && onRecipeUnlockHoldCue != null)
        {
            holdAudioSource.clip = onRecipeUnlockHoldCue.audioClip;
            holdAudioSource.volume = onRecipeUnlockHoldCue.volume;
            holdAudioSource.pitch = onRecipeUnlockHoldCue.pitch;
            holdAudioSource.playOnAwake = false;
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromInputs();
    }

    /// <summary>
    /// Called by Recipe Book Tabs Manager
    /// </summary>
    public void OnTabSwitched()
    {
        if (_tabsManager == null) return;

        int currentIndex = _tabsManager.currentTabIndex;

        if (currentIndex < 0 || currentIndex >= _tabsManager.tabs.Length)
            return;

        RecipeFilterCategory filter = _tabsManager.tabs[currentIndex].category;

        _isSwitchingTabs = true;
        LoadAndRefreshList(filter, true);
        _isSwitchingTabs = false;
    }

    /// <summary>
    /// Load data and refresh view
    /// </summary>
    /// <param name="resetSelection"></param>
    private void LoadAndRefreshList(RecipeFilterCategory filter, bool resetSelection)
    {
        if (RecipeBookManager.instance != null)
        {
            var filteredRecipes = RecipeBookManager.instance.GetFilteredRecipes(filter);

            // Control the animation direction
            int direction = (_isSwitchingTabs && _tabsManager != null) ? _tabsManager.navigationDirection : 0;

            _recipeBookView.AnimateAndRefresh(filteredRecipes, resetSelection, direction);
        }
        else
        {
            Debug.LogError("[RecipeBookController] RecipeBookManager Missing");
        }
    }

    #region Interface (IMenuPage)
    public void OpenPage() 
    {
        gameObject.SetActive(true);

        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.SetNavigationMode(false);
        }

        RecipeSlotAnimator.OnRecipeItemSelected += HandleRecipeSelected;
        SubscribeToInputs();

        if (recipeChannel != null)
        {
            recipeChannel.OnRecipeDataChanged += HandleRecipeDataChanged;
        }

        if (_tabsManager != null) _tabsManager.ResetUI();
        if (_stateManager != null) _stateManager.ResetUI();

        // Load All by default
        LoadAndRefreshList(RecipeFilterCategory.All, true);

        _currentState = RecipeBookState.Navigating;
        // Select first item
        if (_recipeBookView != null)
        {
            StartCoroutine(_recipeBookView.SelectFirstItemAfterDelay(true));
        }
    }

    public void ClosePage()
    {
        _navigationHandler.StopContinuousNavigation();

        RecipeSlotAnimator.OnRecipeItemSelected -= HandleRecipeSelected;

        UnsubscribeFromInputs();

        if (recipeChannel != null)
        {
            recipeChannel.OnRecipeDataChanged -= HandleRecipeDataChanged;
        }

        if (_cancelAnimationCoroutine != null)
        {
            StopCoroutine(_cancelAnimationCoroutine);
            _cancelAnimationCoroutine = null;
        }

        if (_unlockTimerCoroutine != null)
        {
            StopCoroutine(_unlockTimerCoroutine);
            _unlockTimerCoroutine = null;
        }

        _isSwitchingTabs = false;

        gameObject.SetActive(false);
    }

    public void ResetPage()
    {
        ResetUI();
    }

    public void ResetUI()
    {
        if (_tabsManager != null) _tabsManager.ResetUI();
        if (_stateManager != null) _stateManager.ClearSelection();
    }
    #endregion

    #region Event subscription
    private void SubscribeToInputs()
    {
        if (inputChannel != null)
        {
            inputChannel.OnNavigate += HandleNavigation;

            inputChannel.OnNavigateLeft += HandleNavigateLeft;
            inputChannel.OnNavigateRight += HandleNavigateRight;

            inputChannel.OnConfirmStarted += HandleConfirmStarted;
            inputChannel.OnConfirmPerformed += OnConfirmPerformed;
            inputChannel.OnConfirmCanceled += OnConfirmCanceled;
        }
    }
    private void UnsubscribeFromInputs()
    {
        if (inputChannel != null)
        {
            inputChannel.OnNavigate -= HandleNavigation;

            inputChannel.OnNavigateLeft -= HandleNavigateLeft;
            inputChannel.OnNavigateRight -= HandleNavigateRight;

            inputChannel.OnConfirmStarted -= HandleConfirmStarted;
            inputChannel.OnConfirmPerformed -= OnConfirmPerformed;
            inputChannel.OnConfirmCanceled -= OnConfirmCanceled;
        }
    }
    #endregion

    #region Input Functions
    /// <summary>
    /// Navigation input passed by MenuController
    /// </summary>
    private void HandleNavigation(InputAction.CallbackContext context)
    {
        if (_currentState != RecipeBookState.Navigating) return;

        if (!gameObject.activeInHierarchy || (inputChannel != null && inputChannel.IsInputLocked) || UserInput.IsRadialMenuHeldDown) return;

        Vector2 move = context.ReadValue<Vector2>();

        if (context.performed)
        {
            _navigationHandler.MoveOneStep(move);
            _navigationHandler.StartContinuousNavigation(move);
        }

        if (context.canceled)
        {
            _navigationHandler.StopContinuousNavigation();
        }
    }
    private void HandleRecipeSelected(GameObject selectedObject, int index)
    {
        if (_cancelAnimationCoroutine != null)
        {
            StopCoroutine(_cancelAnimationCoroutine);
            _cancelAnimationCoroutine = null;
        }

        _stateManager.OnItemSelected(selectedObject, index);

        _currentSelectedRecipeStatus = null;
        _currentSelectedSlotUI = null;

        RecipeSlotUI slotUI = selectedObject.GetComponent<RecipeSlotUI>();

        if (slotUI != null && slotUI.IData != null)
        {
            _currentSelectedRecipeStatus = slotUI.IData;     // Cache recipe status
            _currentSelectedSlotUI = slotUI;                 // Cache slotUI componenet

            if (_currentSelectedRecipeStatus.isNew)
                RecipeBookManager.instance.MarkRecipeViewed(_currentSelectedRecipeStatus);
        }

        // Show details content
        if (_recipeDetailsView == null) return;

        Recipe recipeToDisplay = null;
        bool isLocked = true;
        if (_currentSelectedRecipeStatus != null)
        {
            recipeToDisplay = _currentSelectedRecipeStatus.recipe;
            isLocked = !_currentSelectedRecipeStatus.isUnlocked;
        }

        foreach (var view in _recipeDetailsView)
        {
            if (view != null)
            {
                view.DisplayRecipe(recipeToDisplay);
            }
        }

        // Update blur effect
        if (_recipePanelLogic != null)
        {
            if (isLocked)
            {
                _recipePanelLogic.LockRecipe(recipeToDisplay);
            }
            else
            {
                _recipePanelLogic.ShowUnlockedDetails();
            }
        }
    }
    private void HandleNavigateLeft(InputAction.CallbackContext context)
    {
        if (_currentState != RecipeBookState.Navigating) return;

        if (!gameObject.activeInHierarchy || (inputChannel != null && inputChannel.IsInputLocked))
        {
            return;
        }

        if (_tabsManager == null) return;

        _tabsManager.NavigateTabs(-1);
    }
    private void HandleNavigateRight(InputAction.CallbackContext context)
    {
        if (_currentState != RecipeBookState.Navigating) return;

        if (!gameObject.activeInHierarchy || (inputChannel != null && inputChannel.IsInputLocked))
        {
            return;
        }

        if (_tabsManager == null) return;

        _tabsManager.NavigateTabs(1);
    }
    #endregion

    #region Unlocking animation processing logic
    private void HandleConfirmStarted(InputAction.CallbackContext context)
    {
        if (_currentState != RecipeBookState.Navigating) return;

        // Guard: Prevents re-triggering while already holding
        if (_isHoldToUnlockActive) return; 

        if (_cancelAnimationCoroutine != null)
        {
            // If the "cancel" animation is playing, ignore this new "start" input.
            Debug.Log("[RecipeBookController] Cancel animation is playing, ignoring this 'Start' input.");
            return;
        }

        // Security check
        if (!gameObject.activeInHierarchy || (inputChannel != null && inputChannel.IsInputLocked) || _currentSelectedSlotUI == null || _currentSelectedRecipeStatus == null) return;
        if (_currentSelectedRecipeStatus.isUnlocked) return;

        if (inputChannel != null)
        {
            inputChannel.RaiseGlobalInputLockEvent(true);
        }

        // Check if it satisfies the inspiration counts
        int cost = _currentSelectedRecipeStatus.recipe.inspirationCost;
        if (!RecipeBookManager.instance.CanAfford(cost))
        {
            Debug.Log($"[RecipeBookController] Not enough inspiration! Need: {cost}");

            OnUnlockFailed?.Invoke();

            if (_currentSelectedSlotUI != null)
            {
                _currentSelectedSlotUI.transform.DOKill(true);
                _currentSelectedSlotUI.transform.DOShakePosition(0.3f, strength: 5f, vibrato: 20);
            }
            inputChannel.RaiseGlobalInputLockEvent(false);
            return;
        }

        PlayHoldAudio();

        // Set state to active
        _currentState = RecipeBookState.HoldingToUnlock;
        _isHoldToUnlockActive = true;

        // --- Particle System START ---
        _currentUnlockDuration = 1.0f; // default value
        if (_currentSelectedSlotUI.UnlockHoldAnimation != null)
        {
            _currentUnlockDuration = _currentSelectedSlotUI.UnlockHoldAnimation.duration;
        }

        // Notify the View: Start firing particles towards the current slot for 'duration' seconds
        _currentSelectedSlotUI.StartUnlockVisuals();
        OnUnlockChargeStart?.Invoke(_currentSelectedSlotUI.transform, _currentUnlockDuration);

        if (_unlockTimerCoroutine != null)
        {
            StopCoroutine(_unlockTimerCoroutine);
        }
        _unlockTimerCoroutine = StartCoroutine(UnlockTimerCoroutine(_currentUnlockDuration));
        // --- Particle System END ---
    }

    private void OnConfirmPerformed(InputAction.CallbackContext context)
    {
        // Do nothing
    }

    private IEnumerator UnlockTimerCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        HandleAutoUnlock();
    }

    private void HandleAutoUnlock()
    {
        ResetHoldState(false);

        _currentState = RecipeBookState.Navigating;
        _isHoldToUnlockActive = false;

        bool success = RecipeBookManager.instance.UnlockRecipe(_currentSelectedRecipeStatus);
        if (success)
        {
            Debug.Log($"[RecipeBookController] Recipe unlocked.");

            _currentSelectedSlotUI.OnSelect(null);
            _currentSelectedSlotUI.PlayUnlockSuccessVisuals();

            if (uiAudioChannel != null && onRecipeUnlockCue != null)
            {
                uiAudioChannel.RaiseEvent(onRecipeUnlockCue);
            }
            if (_recipePanelLogic != null)
            {
                _recipePanelLogic.UnlockRecipe();
            }
        }
        else
        {
            _currentSelectedSlotUI.CancelUnlockVisuals();
        }
    }

    private void OnConfirmCanceled(InputAction.CallbackContext context)
    {
        // Always trigger cancel event on release
        OnUnlockChargeCancel?.Invoke();

        if (!_isHoldToUnlockActive) return;

        if (_unlockTimerCoroutine != null)
        {
            StopCoroutine(_unlockTimerCoroutine);
            _unlockTimerCoroutine = null;
        }

        ResetHoldState(true);

        _currentState = RecipeBookState.Navigating;
        _isHoldToUnlockActive = false;

       if (_currentSelectedSlotUI != null)
       {
            // Released the button too early
            Debug.Log($"[RecipeBookController] Canceled: Animation did not finish");
            _currentSelectedSlotUI.CancelUnlockVisuals();
            if (_cancelAnimationCoroutine != null)
            {
                StopCoroutine(_cancelAnimationCoroutine);
            }
            _cancelAnimationCoroutine = StartCoroutine(CancelStateUnlockCoroutine(_currentUnlockDuration));
       }
    }

    /// <summary>
    /// Prevent animation conflicts caused by pressing again 
    /// when the player cancels the animation playback
    /// </summary>
    private IEnumerator CancelStateUnlockCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        _cancelAnimationCoroutine = null;
    }

    #endregion

    #region Internal Methods
    private void ResetHoldState(bool fadeAudio)
    {
        if (inputChannel != null) inputChannel.RaiseGlobalInputLockEvent(false);

        if (holdAudioSource != null)
        {
            holdAudioSource.DOKill();
            if (fadeAudio && holdAudioSource.isPlaying)
            {
                holdAudioSource.DOFade(0, 0.25f).SetEase(Ease.OutQuad).OnComplete(() => holdAudioSource.Stop());
            }
            else
            {
                holdAudioSource.Stop();
            }
        }
        _unlockTimerCoroutine = null;
    }
    private void PlayHoldAudio()
    {
        if (holdAudioSource != null)
        {
            holdAudioSource.DOKill();
            if (onRecipeUnlockHoldCue != null)
            {
                holdAudioSource.volume = onRecipeUnlockHoldCue.volume;
            }
            holdAudioSource.Play();
        }
    }
    #endregion

    #region DEBUG 
    private void HandleRecipeDataChanged()
    {
        if (_tabsManager == null) return;
        int currentIndex = _tabsManager.currentTabIndex;
        if (currentIndex < 0 || currentIndex >= _tabsManager.tabs.Length) return;
        RecipeFilterCategory filter = _tabsManager.tabs[currentIndex].category;

        _isSwitchingTabs = false;
        LoadAndRefreshList(filter, false);
    }
    #endregion
}

