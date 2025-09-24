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

        // ǿ���������²��֣�ȷ������ RectTransform ���Ѽ������
        // ����� Screen Space - Camera ģʽ�µĳ�ʼ��������������Ч
        if (selectBackground != null && selectBackground.transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(selectBackground.transform.parent as RectTransform);
        }

        // ��һ�δ�ʱ��ֱ������λ�ã������Ŷ���
        SelectTab(tabs[0], false);
    }

    // ���һ����ѡ�� animate ������Ĭ��Ϊ true
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
            if (animate) // ��� animate Ϊ true���򲥷Ŷ���
            {
                selectBackground.transform.DOMove(selectedTab.transform.position, 0.2f)
                            .SetEase(Ease.OutQuad);
            }
            else // ����ֱ������λ��
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
            SelectTab(tabs[newIndex], true); // �û�����ʱ�����Ŷ���
        }
    }

    // Reset the filter page options when opening the menu 
    public void SelectTab(int selectTabIndex)
    {
        // ��������� MenuController ���ã����ڳ�ʼ�������ã�����Ҳ�����Ŷ���
        SelectTab(tabs[selectTabIndex], false);
    }
}
