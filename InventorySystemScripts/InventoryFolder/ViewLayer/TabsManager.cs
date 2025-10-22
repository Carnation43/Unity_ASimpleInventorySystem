using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using InstanceResetToDefault;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Manages the behaivor of the tab group in the inventory menu.
/// It handles the selection, navigation via input, and visual feedback like animations and cooldowns.
/// </summary>
public class TabsManager : MonoBehaviour, IResettable
{
    [Header("Input Listening To")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("SFX Broadcasting On")]
    [SerializeField] private AudioCueEventChannel uiAudioChannel;
    [SerializeField] private AudioCueSO onTabSwitchCue;

    [SerializeField] UnityEvent<int> onTabSelected; 

    [Header("Visual Effect Components")]
    [SerializeField] Image selectBackground;
    [SerializeField] Image leftArrow;
    [SerializeField] Image rightArrow;
    [SerializeField] TextMeshProUGUI subheading;
    [SerializeField] float cooldownDuration = 0.2f; // used to avoid pressing consecutively

    public Tab[] tabs;

    public int currentTabIndex { get; private set; } = 0;
    public int navigationDirection { get; private set; } = 0; // the track for navigating left or right
    private bool _isCoolingDown = false; // a flag to prevent input when an animation is in progress
    private bool _isInputLocked = false;

    private void Awake()
    {
        tabs = GetComponentsInChildren<Tab>();

        foreach (var tab in tabs)
        {
            tab.Initialize(this);
        }
    }

    private void OnEnable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.Register(this);
        }
        if (inputChannel != null)
        {
            inputChannel.OnNavigateLeft += HandleNavigateLeft;
            inputChannel.OnNavigateRight += HandleNavigateRight;
            inputChannel.OnGlobalInputLock += HandleInputLock;
        }
    }

    private void OnDisable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.UnRegister(this);
        }
        if (inputChannel != null)
        {
            inputChannel.OnNavigateLeft -= HandleNavigateLeft;
            inputChannel.OnNavigateRight -= HandleNavigateRight;
            inputChannel.OnGlobalInputLock -= HandleInputLock;
        }
    }

    private void Start()
    {
        // Force update layout
        if (selectBackground != null && selectBackground.transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(selectBackground.transform.parent as RectTransform);
        }

        if (tabs != null && tabs.Length > 0)
        {
            SelectTab(tabs[0], false);
        }
    }

    private void HandleInputLock(bool isLocked)
    {
        Debug.Log(gameObject.name + " received lock event: " + isLocked);
        _isInputLocked = isLocked;
    }

    private IEnumerator CoolingDownCoroutine()
    {
        _isCoolingDown = true;
        yield return new WaitForSeconds(cooldownDuration);
        _isCoolingDown = false;
    }

    private void HandleNavigateLeft(InputAction.CallbackContext context)
    {
        if (MenuController.instance == null || MenuController.instance.currentFocus != MenuController.MenuFocus.Inventory)
        {
            return;
        }

        if (_isInputLocked || UserInput.IsRadialMenuHeldDown) return;

        NavigateTabs(-1);
    }

    private void HandleNavigateRight(InputAction.CallbackContext context)
    {
        if (MenuController.instance == null || MenuController.instance.currentFocus != MenuController.MenuFocus.Inventory)
        {
            return;
        }

        if (_isInputLocked || UserInput.IsRadialMenuHeldDown) return;

        NavigateTabs(1);
    }

    public void SelectTab(Tab selectedTab, bool animate = true)
    {
        int selectedSiblingIndex = -1;

        // ---------------- record move direction -------------------
        int newIndex = -1;
        for(int i = 0; i < tabs.Length; i++)
        {
            if(tabs[i] == selectedTab)
            {
                newIndex = i;
                break;
            }
        }
        if(newIndex != currentTabIndex)
        {
            navigationDirection = newIndex > currentTabIndex ? 1 : -1;
        }
        else
        {
            navigationDirection = 0;
        }
        // ----------------------------------------------------------

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == selectedTab)
            {
                currentTabIndex = i; // update currentTabIndex 
                tabs[i].OnSelect();
                selectedSiblingIndex = tabs[i].transform.GetSiblingIndex();
                if (animate)
                {
                    ChangeSubheadingText(selectedTab);
                }
                else
                {
                    if (subheading != null)
                    {
                        subheading.text = selectedTab.category.ToString();
                        subheading.alpha = 1;
                    }
                }
            }
            else
            {
                tabs[i].OnDeselect();
            }
        }
        // Animation effect 
        if (selectBackground != null)
        {
            if (animate)
            {
                selectBackground.transform.DOMove(selectedTab.transform.position, 0.2f)
                            .SetEase(Ease.OutQuad);
            }
            else 
            {
                selectBackground.transform.position = selectedTab.transform.position;
            }
        }
        onTabSelected?.Invoke(selectedSiblingIndex - 1); // Subtracting 1 is because selectBackground occupies the position of Sibling Index 0. 
    }

    /// <summary> 
    /// navigate by the direction 
    /// </summary> 
    /// <param name="direction">-1 -- left, 1 -- right</param> 
    public void NavigateTabs(int direction)
    {
        int newIndex = currentTabIndex + direction;
        int arrowIndex = newIndex;

        // Ensure that it does not go beyond the scope 
        newIndex = Mathf.Clamp(newIndex, 0, tabs.Length - 1);

        if (newIndex != currentTabIndex)
        {
            if (_isCoolingDown) return;
            StartCoroutine(CoolingDownCoroutine());
            if (uiAudioChannel != null && onTabSwitchCue != null)
            {
                uiAudioChannel.RaiseEvent(onTabSwitchCue);
            }
            SelectTab(tabs[newIndex], true); // play animation when navigating inventory
        }

        if(direction < 0)
        {
            leftArrow.rectTransform.DOKill();
            if(arrowIndex >= 0)
            {
                leftArrow.rectTransform.DOScale(Vector2.one * 1.3f, 0);
                leftArrow.rectTransform.DOScale(Vector2.one, 0.1f);
            }
            else
            {
                leftArrow.rectTransform.DOShakePosition(0.15f, new Vector3(1.0f, 0, 0), 10, 0, false, true);
            }
         
        }
        else
        {
            rightArrow.rectTransform.DOKill();
            if (arrowIndex <= tabs.Length - 1)
            {
                rightArrow.rectTransform.DOScale(Vector2.one * 1.3f, 0);
                rightArrow.rectTransform.DOScale(Vector2.one, 0.1f);
            }
            else
            {
                rightArrow.rectTransform.DOShakePosition(0.15f, new Vector3(1.0f, 0, 0), 10, 0, false, true);
            }
        }
    }

    public void ChangeSubheadingText(Tab selectedTab)
    {
        Sequence subheadingSequence = DOTween.Sequence();
        subheadingSequence.Append(subheading.DOFade(0, 0.1f));
        subheadingSequence.AppendCallback(() => subheading.text = selectedTab.category.ToString());
        subheadingSequence.Append(subheading.DOFade(1, 0.1f));
    }

    public void ResetToDefaultState()
    {
        SelectTab(tabs[0], false);
        _isCoolingDown = false;
        currentTabIndex = 0;
        navigationDirection = 0;
    }
}