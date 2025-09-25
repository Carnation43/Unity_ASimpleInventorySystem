using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tab : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Image icon;
    // [SerializeField] Image background;

    // Temp
    public TabsManager tabsManager;
    public ItemCategory category;

    public void Initialize(TabsManager tabsManager)
    {
        this.tabsManager = tabsManager;
    }

    public void OnSelect()
    {
        icon.color = Color.black;
    }

    public void OnDeselect()
    {
        icon.color = Color.white;
    }

    public void OnClick()
    {
        tabsManager.SelectTab(this);
    }
}
