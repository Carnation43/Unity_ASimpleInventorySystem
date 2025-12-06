using DG.Tweening;
using InstanceResetToDefault;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the high-level view elements of the Side Menu.
/// Handles the visibility of the entire menu.
/// </summary>
public class SideMenuView : MonoBehaviour, IResettableUI
{
    [Header("Container Control")]
    [SerializeField] private CanvasGroup sideMenuContainer;

    [Header("Header Info")]
    [SerializeField] private TMP_Text locationNameText;
    [SerializeField] private TMP_Text subLocationNameText;

    [Header("Buttons List")]
    [SerializeField] private List<SideMenuButton> sideMenuButtons;

    public List<SideMenuButton> Buttons => sideMenuButtons;

    private void Awake()
    {
        SetMenuVisibility(false, 0f);

        if (sideMenuButtons == null)
        {
            sideMenuButtons = new List<SideMenuButton>(GetComponentsInChildren<SideMenuButton>(true));
        }
    }

    public void UpdateLoactionInfo(string location, string subLocation)
    {
        if (locationNameText != null) locationNameText.text = location;
        if (subLocationNameText != null) subLocationNameText.text = subLocation;
    }

    public void SetMenuVisibility(bool isVisible, float duration = 0.2f)
    {
        if (sideMenuContainer == null) return;

        sideMenuContainer?.DOKill();

        if (isVisible)
        {
            sideMenuContainer.gameObject.SetActive(true);
            sideMenuContainer.interactable = true;
            sideMenuContainer.blocksRaycasts = true;
            sideMenuContainer.DOFade(1f, duration).SetUpdate(true);
        }
        else
        {
            sideMenuContainer.interactable = false;
            sideMenuContainer.blocksRaycasts = false;
            sideMenuContainer.DOFade(0f, duration).SetUpdate(true).OnComplete(() =>
            {
                sideMenuContainer.gameObject.SetActive(false);
            });
        }
    }

    /// <summary>
    /// Forces the EventSystem to select the first available button.
    /// </summary>
    public void SelectFirstButton()
    {
        if (sideMenuButtons != null && sideMenuButtons.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(sideMenuButtons[0].gameObject);
        }
    }

    public void ResetUI()
    {
        SetMenuVisibility(false, 0);
    }
}
