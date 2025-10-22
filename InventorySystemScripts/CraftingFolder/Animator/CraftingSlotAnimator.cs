using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftingSlotAnimator : BaseIconAnimationController
{
    public override void OnSelect(BaseEventData eventData)
    {
        if (selected) return;
        if (canvasGroup == null || rectTransform == null) return;
        selected = true;

        canvasGroup.DOFade(1, 0.2f);
        rectTransform.DOScale(Vector2.one * 1.1f, 0.2f);
        rectTransform.DOLocalRotate(new Vector3(0, 0, 10), 0.4f).SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            if (selected)
                rectTransform.DOLocalRotate(new Vector3(0, 0, -10), 0.8f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        });
    }
}
