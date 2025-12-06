using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// This script may deplete in the future
/// </summary>
public class EquipmentsIconAnimationController : BaseIconAnimationController
{
    public override void OnSelect(BaseEventData eventData)
    {
        if (canvasGroup == null || rectTransform == null || imageBackground == null)
        {
            return;
        }

        selected = true;

        canvasGroup.DOFade(1, 0.2f);
        rectTransform.DOScale(Vector2.one * 1.2f, 0.2f);

        imageBackground.transform.DOKill();
        imageBackground.transform.DOLocalRotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental); 
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData); 

        if (imageBackground != null)
        {
            imageBackground.transform.DOKill();
            imageBackground.transform.DOLocalRotate(Vector3.zero, 0.25f); 
        }
    }
}