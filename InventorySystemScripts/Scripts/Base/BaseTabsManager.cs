using DG.Tweening;
using InstanceResetToDefault;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Abstract class that handles common logic for tab navigation (Next/Previous)
/// selection visuals, audio feedback, and cooldowns.
/// T_Tab: The specific type of Tab component
/// T_Category: The enum type used for categorization.
/// </summary>
/// <typeparam name="T_Tab"></typeparam>
/// <typeparam name="T_Category"></typeparam>
public abstract class BaseTabsManager<T_Tab, T_Category> : MonoBehaviour, IResettableUI
    where T_Tab : BaseTab<T_Category>
    where T_Category : Enum
{ 
    [Header("SFX Broadcasting On")]
    [SerializeField] protected AudioCueEventChannel uiAudioChannel;
    [SerializeField] protected AudioCueSO onTabSwitchCue;

    [Header("Inspector Unity Event")]
    [SerializeField] public UnityEvent onTabSelected;

    [Header("Visual Effect Components")]
    [SerializeField] protected Image selectBackground;
    [SerializeField] protected Image leftArrow;
    [SerializeField] protected Image rightArrow;
    [SerializeField] protected TextMeshProUGUI subheading;
    [SerializeField] protected float cooldownDuration = 0.2f; // used to avoid pressing consecutively

    public T_Tab[] tabs;

    public int currentTabIndex { get; protected set; } = 0;
    public int navigationDirection { get; protected set; } = 0; // the track for navigating left or right
    protected bool _isCoolingDown = false; // a flag to prevent input when an animation is in progress


    protected abstract void InitializeTabs();

    protected virtual void Awake()
    {
        InitializeTabs();
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    protected virtual void Start()
    {
        if (selectBackground != null && selectBackground.transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(selectBackground.transform.parent as RectTransform);
        }

        if (tabs != null && tabs.Length > 0)
        {
            SelectTab(tabs[0], false);
        }
    }

    protected IEnumerator CoolingDownCoroutine()
    {
        _isCoolingDown = true;
        yield return new WaitForSeconds(cooldownDuration);
        _isCoolingDown = false;
    }

    public virtual void SelectTab(T_Tab selectedTab, bool animate = true, bool triggerEvent = true)
    {

        // ---------------- record move direction -------------------
        int newIndex = -1;
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == selectedTab)
            {
                newIndex = i;
                break;
            }
        }
        if (newIndex != currentTabIndex)
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
                currentTabIndex = i;
                tabs[i].OnSelect();
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

        if (selectBackground != null)
        {
            Vector3 targetLocalPos = selectBackground.transform.parent.InverseTransformPoint(selectedTab.transform.position);

            if (animate)
            {
                selectBackground.transform.DOLocalMove(targetLocalPos, 0.2f)
                            .SetEase(Ease.OutQuad);
            }
            else
            {
                selectBackground.transform.localPosition = targetLocalPos;
            }
        }

        if (triggerEvent)
            onTabSelected?.Invoke(); 
    }

    /// <summary> 
    /// navigate by the direction 
    /// </summary> 
    /// <param name="direction">-1 -- left, 1 -- right</param> 
    public virtual void NavigateTabs(int direction)
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

        if (direction < 0)
        {
            leftArrow.rectTransform.DOKill();
            if (arrowIndex >= 0)
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

    public virtual void ChangeSubheadingText(T_Tab selectedTab)
    {
        if (subheading == null) return;

        Sequence subheadingSequence = DOTween.Sequence();
        subheadingSequence.Append(subheading.DOFade(0, 0.1f));
        subheadingSequence.AppendCallback(() => subheading.text = selectedTab.category.ToString());
        subheadingSequence.Append(subheading.DOFade(1, 0.1f));
    }

    public virtual void ResetUI()
    {
        _isCoolingDown = false;
        currentTabIndex = 0;
        navigationDirection = 0;

        if (tabs != null && tabs.Length > 0)
        {
            SelectTab(tabs[0], false, false); // (false means no animation)
        }
    }
}
