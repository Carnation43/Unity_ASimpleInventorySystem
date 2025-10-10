using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Controls the open and close animations for the main inventory menu canvas.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class MenuAnimator : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Sequence _currentAnimation;

    public bool IsAnimating { get; private set; }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();

        _canvasGroup.alpha = 0;
    }

    public Coroutine PlayOpenAnimation()
    {
        return StartCoroutine(OpenAnimationCoroutine());
    }

    private IEnumerator OpenAnimationCoroutine()
    {
        _currentAnimation?.Kill();
        IsAnimating = true;

        gameObject.SetActive(true);

        _canvasGroup.alpha = 0;
        _rectTransform.localScale = Vector3.one * 0.95f;

        _currentAnimation = DOTween.Sequence();
        _currentAnimation.Append(_canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad));
        _currentAnimation.Join(_rectTransform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        _currentAnimation.OnComplete(() => IsAnimating = false);

        yield return _currentAnimation.WaitForCompletion();
    }


    public Coroutine PlayCloseAnimation()
    {
        return StartCoroutine(CloseAnimationCoroutine());
    }

    private IEnumerator CloseAnimationCoroutine()
    {
        _currentAnimation?.Kill();
        IsAnimating = true;

        _currentAnimation = DOTween.Sequence();
        _currentAnimation.Append(_canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad));
        _currentAnimation.Join(_rectTransform.DOScale(Vector3.one * 0.95f, 0.2f).SetEase(Ease.InBack));
        _currentAnimation.OnComplete(() =>
        {
            IsAnimating = false;
        });

        yield return _currentAnimation.WaitForCompletion();
    }

    private void OnDestroy()
    {
        _currentAnimation?.Kill();
    }
}