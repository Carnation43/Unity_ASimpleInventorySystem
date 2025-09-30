using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using InstanceResetToDefault;

public class TabsManager : MonoBehaviour, IResettable
{
    [SerializeField] UnityEvent<int> onTabSelected;

    [Header("Visual Effect Components")]
    [SerializeField] Image selectBackground;
    [SerializeField] Image leftArrow;
    [SerializeField] Image rightArrow;
    [SerializeField] TextMeshProUGUI subheading;

    Tab[] tabs;

    public int currentTabIndex { get; private set; } = 0;

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
    }

    private void OnDisable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.UnRegister(this);
        }
    }

    private void Start()
    {
        // Force update layout
        if (selectBackground != null && selectBackground.transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(selectBackground.transform.parent as RectTransform);
        }
    }

    
    public void SelectTab(Tab selectedTab, bool animate = true)
    {
        int selectedSiblingIndex = -1;

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == selectedTab)
            {
                currentTabIndex = i; // update currentTabIndex 
                tabs[i].OnSelect();
                selectedSiblingIndex = tabs[i].transform.GetSiblingIndex();
                ChangeSubheadingText(selectedTab);
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
                selectBackground.rectTransform.pivot = selectedTab.gameObject.GetComponent<RectTransform>().pivot;
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
    }
}