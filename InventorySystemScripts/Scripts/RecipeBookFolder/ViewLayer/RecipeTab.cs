using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single tab in the Recipe Book.
/// </summary>
public class RecipeTab : BaseTab<RecipeFilterCategory>
{
    private RecipeBookTabsManager tabsManager;

    [Header("References")]
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TMP_Text recipeTabText;

    [Header("Animation Settings")]
    [SerializeField] private float doScale = 1.2f;
    [SerializeField] private float scaleTime = 0.15f;

    private void Awake()
    {
        tooltip.gameObject.SetActive(false);
        recipeTabText.text = category.ToString();
    }

    public void Initialize(RecipeBookTabsManager manager)
    {
        this.tabsManager = manager;
    }

    public override void OnSelect()
    {
        base.OnSelect();
        tooltip.gameObject.SetActive(true);

        transform.DOKill();
        icon.transform.DOScale(Vector3.one * doScale, scaleTime).SetEase(Ease.OutQuad);
    }

    public override void OnDeselect()
    {
        base.OnDeselect();
        tooltip.gameObject.SetActive(false);

        icon.transform.DOScale(Vector3.one, scaleTime).SetEase(Ease.OutQuad);
    }

    public override void OnClick()
    {
        if (tabsManager != null)
        {
            // tabsManager.SelectTab(this);
        }
    }
}
