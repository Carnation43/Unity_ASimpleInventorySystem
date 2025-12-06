using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the visual fill animation of the crafting arrow.
/// </summary>
public class CraftingAnimator : MonoBehaviour
{
    [SerializeField] private Image craftingArrowFill;

    [SerializeField] private float longCraftDuration = 0.3f;

    private Tweener _fillTween;

    public float FillAmount => craftingArrowFill.fillAmount;

    private void Awake()
    {
        if (craftingArrowFill != null)
        {
            craftingArrowFill.fillAmount = 0;
        }
    }

    public void PlaySingleCraftAnimation()
    {
        if (craftingArrowFill == null) return;

        craftingArrowFill.DOKill();
        craftingArrowFill.fillAmount = 0;

        // Fast flash effect for crafting single item
        craftingArrowFill.DOFillAmount(1, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
            craftingArrowFill.fillAmount = 0;
        });
    }

    public void StartLongCraftAnimation()
    {
        if (craftingArrowFill == null) return;
        _fillTween?.Kill();
        craftingArrowFill.fillAmount = 0;
        _fillTween = craftingArrowFill.DOFillAmount(1, longCraftDuration).SetEase(Ease.Linear);
    }

    public void CancelLongCraftAnimation()
    {
        if (craftingArrowFill == null) return;
        _fillTween?.Kill();
        craftingArrowFill.DOFillAmount(0, 0.1f);
    }

    private void OnDestroy()
    {
        craftingArrowFill.DOKill();
        _fillTween?.Kill();
    }
}
