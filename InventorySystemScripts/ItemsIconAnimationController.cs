using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UIElements;

public class ItemsIconAnimationController : BaseIconAnimationController
{
    // Awake, OnDeselect, OnPointerEnter, OnPointerExit has been handled in BaseIconAnimationController

    public override void OnSelect(BaseEventData eventData)
    {
        if (canvasGroup == null || rectTransform == null)
        {
            return;
        }

        selected = true;

        // swing left and right

        // record index and gameObject on last selected item
        MenuController.instance.LastItemSelected = gameObject;
        for (int i = 0; i < MenuController.instance.inventorySlots.Count; i++)
        {
            if (MenuController.instance.inventorySlots[i].gameObject == gameObject)
            {
                MenuController.instance.LastSelectedIndex = i;
                break;
            }
        }

        canvasGroup.DOFade(1, 0.2f);
        rectTransform.DOScale(Vector2.one * 1.2f, 0.2f);
        rectTransform.DOLocalRotate(new Vector3(0, 0, 10), 0.4f).SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            if (selected)
                rectTransform.DOLocalRotate(new Vector3(0, 0, -10), 0.8f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        });
    }
}