using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabsManager : MonoBehaviour
{
    [SerializeField] UnityEvent<int> onTabSelected;
    [SerializeField] Image selectBackground;

    Tab[] tabs;
    private int currentTabIndex = 0;

    private void Start()
    {
        tabs = GetComponentsInChildren<Tab>();

        foreach (var tab in tabs)
        {
            tab.Initialize(this);
        }

        // 强制立即更新布局，确保所有 RectTransform 都已计算完毕
        // 这对于 Screen Space - Camera 模式下的初始布局问题尤其有效
        if (selectBackground != null && selectBackground.transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(selectBackground.transform.parent as RectTransform);
        }

        // 第一次打开时，直接设置位置，不播放动画
        SelectTab(tabs[0], false);
    }

    // 添加一个可选的 animate 参数，默认为 true
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
            }
            else
            {
                tabs[i].OnDeselect();
            }
        }
        // Animation effect 
        if (selectBackground != null)
        {
            if (animate) // 如果 animate 为 true，则播放动画
            {
                selectBackground.transform.DOMove(selectedTab.transform.position, 0.2f)
                            .SetEase(Ease.OutQuad);
            }
            else // 否则，直接设置位置
            {
                selectBackground.transform.position = selectedTab.transform.position;
            }
        }
        onTabSelected?.Invoke(selectedSiblingIndex - 1); // Subtracting 1 is because selectBackground occupies the position of Sibling Index 0. 
    }

    /// <summary> 
    /// navigate by the direction 
    /// </summary> 
    /// <param name="direction">1 -- left, -1 -- right</param> 
    public void NavigateTabs(int direction)
    {
        int newIndex = currentTabIndex + direction;

        // Ensure that it does not go beyond the scope 
        newIndex = Mathf.Clamp(newIndex, 0, tabs.Length - 1);

        if (newIndex != currentTabIndex)
        {
            SelectTab(tabs[newIndex], true); // 用户导航时，播放动画
        }
    }

    // Reset the filter page options when opening the menu 
    public void SelectTab(int selectTabIndex)
    {
        // 这个方法被 MenuController 调用，用于初始化或重置，所以也不播放动画
        SelectTab(tabs[selectTabIndex], false);
    }
}
