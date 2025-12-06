using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Controls the open and close animations for the main inventory menu canvas.
/// Adjusted for FULL SCREEN Sad/Melancholic atmosphere: Slow fade + Subtle Zoom.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class MenuAnimator : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Sequence _currentAnimation;

    public bool IsAnimating { get; private set; }

    [Header("Atmosphere Settings")]
    [Tooltip("打开动画的时长，建议设置在 0.8 到 1.5 秒之间，越慢越沉重")]
    [SerializeField] private float openDuration = 1.2f;
    [Tooltip("关闭动画的时长")]
    [SerializeField] private float closeDuration = 0.8f;

    [Header("Scale Effect")]
    [Tooltip("初始缩放倍率。1.05 表示打开时比屏幕稍大一点点，然后慢慢缩回正常大小")]
    [SerializeField] private float startScale = 1.05f;

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

        // --- 初始化状态 ---
        _canvasGroup.alpha = 0;
        // 设置为稍微放大一点的状态，这样就不会露出边缘
        _rectTransform.localScale = Vector3.one * startScale;

        // 确保位置归零（防止之前修改位置的代码有残留影响）
        _rectTransform.anchoredPosition = Vector2.zero;

        _currentAnimation = DOTween.Sequence();

        // 1. 极慢的淡入：InOutSine 曲线让明暗变化如呼吸般自然
        _currentAnimation.Append(_canvasGroup.DOFade(1f, openDuration).SetEase(Ease.InOutSine));

        // 2. 微缩放归位：从 1.05 缩回到 1.0
        // OutQuart 曲线：开始有一点点速度，最后非常缓慢地停止，仿佛沉重的物体落地
        _currentAnimation.Join(_rectTransform.DOScale(Vector3.one, openDuration).SetEase(Ease.OutQuart));

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

        // 1. 淡出
        _currentAnimation.Append(_canvasGroup.DOFade(0f, closeDuration).SetEase(Ease.InOutSine));

        // 2. 微放大离去：关闭时，让画面稍微再放大一点点并消散
        // 这种“离去”的动效会增加一种迷离感
        _currentAnimation.Join(_rectTransform.DOScale(Vector3.one * startScale, closeDuration).SetEase(Ease.OutSine));

        _currentAnimation.OnComplete(() =>
        {
            IsAnimating = false;
            // 重置缩放，保持整洁
            _rectTransform.localScale = Vector3.one;
        });

        yield return _currentAnimation.WaitForCompletion();
    }

    public void SetAlphaInstantly(float alpha)
    {
        // 1. 确保组件已获取 (防止外部调用早于 Awake)
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

        // 2. 杀掉当前正在运行的动画，防止冲突
        _currentAnimation?.Kill();
        IsAnimating = false;

        // 3. 直接赋值
        _canvasGroup.alpha = alpha;

        // 4. 既然是立即显示/隐藏，通常也希望缩放归位
        _rectTransform.localScale = Vector3.one;
        _rectTransform.anchoredPosition = Vector2.zero;
    }

    private void OnDestroy()
    {
        _currentAnimation?.Kill();
    }
}