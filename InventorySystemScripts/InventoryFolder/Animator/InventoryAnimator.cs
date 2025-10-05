using DG.Tweening;
using InstanceResetToDefault;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Compared to controlling the animation effect of each grid individually,
/// this script controls the transparency and position of the grid container
/// </summary>
public class InventoryAnimator : MonoBehaviour, IResettable
{
    private RectTransform _containerRectTransform;
    private CanvasGroup _containerCanvasGroup;
    private Vector2 _originalPosition;

    public bool IsAnimating { get; private set; } = false;

    private void OnEnable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.Register(this);
        }
    }

    private void OnDisable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.UnRegister(this);
        }
    }

    public void Initialize(Transform container)
    {
        _containerRectTransform = container.GetComponent<RectTransform>();
        _containerCanvasGroup = container.GetComponent<CanvasGroup>();

        if (_containerCanvasGroup == null)
        {
            _containerCanvasGroup = container.gameObject.AddComponent<CanvasGroup>();
        }

        if (_containerRectTransform != null)
        {
            _originalPosition = _containerRectTransform.anchoredPosition;
        }
    }

    /// <summary>
    /// A coroutine that manages the entire animation sequence:
    /// old items fade out, grid refreshes, and new items fade in.
    /// </summary>
    /// <param name="direction">1 for right, -1 for left</param>
    /// <param name="onAnimationMiddle">Callback function, execute the refreshAction in InventoryView.cs</param>
    /// <returns></returns>
    public IEnumerator PlaySwitchAnimation(int direction, Action onAnimationMiddle)
    {
        IsAnimating = true;

        if (_containerCanvasGroup == null || _containerRectTransform == null)
        {
            onAnimationMiddle?.Invoke();
            IsAnimating = false; 
            yield break;
        }

        float moveDistance = 100f;
        float duration = 0.2f;

        Vector2 originalPos = _originalPosition;

        DOTween.Sequence()
            .Append(_containerCanvasGroup.DOFade(0, duration))
            .Join(_containerRectTransform.DOAnchorPosX(originalPos.x - direction * moveDistance, duration).SetEase(Ease.InQuad))
            .SetTarget(this);

        yield return new WaitForSeconds(duration);

        // Execute the callback to ensure the grid is refreshed.
        onAnimationMiddle?.Invoke();

        _containerRectTransform.anchoredPosition = new Vector2(originalPos.x + direction * moveDistance, originalPos.y);

        DOTween.Sequence()
           .Append(_containerCanvasGroup.DOFade(1, duration))
           .Join(_containerRectTransform.DOAnchorPosX(originalPos.x, duration).SetEase(Ease.OutQuad))
           .SetTarget(this)
           .OnComplete(() => {
               IsAnimating = false;
           }); ;
    }

    public void KillAllAnimations()
    {
        DOTween.Kill(this);
    }

    public void ResetToDefaultState()
    {
        KillAllAnimations();
        if (_containerCanvasGroup != null)
        {
            _containerCanvasGroup.alpha = 1f; // reset to visible
        }
        if (_containerRectTransform != null)
        {
            _containerRectTransform.anchoredPosition = _originalPosition;
        }
        IsAnimating = false;
    }
}
