using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RecipeSlotAnimator : BaseIconAnimationController
{
    public static event Action<GameObject, int> OnRecipeItemSelected;

    [Header("Animation Settings")]
    private float selectScale = 1.2f;
    private float selectDuration = 0.2f;

    public override void OnSelect(BaseEventData eventData)
    {
        if (selected) return;

        if (rectTransform == null)
        {
            Debug.LogWarning("$[RecipeSlotAnimator] animation references not configured correctly");
            return;
        }

        selected = true;

        int correctIndex = -1;

        if (RecipeBookView.instance != null)
        {
            var slotList = RecipeBookView.instance.SlotUIList;
            for (int i = 0; i < slotList.Count; i++)
            {
                if (slotList[i].gameObject == this.gameObject)
                {
                    correctIndex = i;
                    break;
                }
            }
        }

        OnRecipeItemSelected?.Invoke(gameObject, correctIndex);

        rectTransform.DOKill();

        rectTransform.DOScale(Vector2.one * selectScale, selectDuration).SetEase(Ease.OutBack);
    }

}
