using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

public class ItemsIconAnimationController : BaseIconAnimationController
{
    // Awake, OnDeselect, OnPointerEnter, OnPointerExit has been handled in BaseIconAnimationController

    public static event Action<GameObject, int> OnItemSelected;

    public override void OnSelect(BaseEventData eventData)
    {
        if (canvasGroup == null || rectTransform == null) return;
        selected = true;

        // record index and gameObject on last selected item
        int correctIndex = -1;
        if (InventoryView.instance != null)
        {
            var slotList = InventoryView.instance.SlotUIList;
            for (int i = 0; i < slotList.Count; i++)
            {
                if (slotList[i].gameObject == this.gameObject)
                {
                    correctIndex = i;
                    break;
                }
            }
        }

        OnItemSelected?.Invoke(gameObject, correctIndex);

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