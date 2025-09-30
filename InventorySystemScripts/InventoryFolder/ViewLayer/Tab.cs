using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tab : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Image icon;
    // [SerializeField] Image background;
    [SerializeField] private Color selectedColor = Color.black;
    [SerializeField] private Color deselectedColor = Color.white;

    // Temp
    public TabsManager tabsManager;
    public ItemCategory category;

    public void Initialize(TabsManager tabsManager)
    {
        this.tabsManager = tabsManager;
    }

    public void OnSelect()
    {
        icon.color = selectedColor;
    }

    public void OnDeselect()
    {
        icon.color = deselectedColor;
    }

    public void OnClick()
    {
        tabsManager.SelectTab(this);
    }
}
