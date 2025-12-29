using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Coffee.UIEffects;
using System;
using DG.Tweening;
using TMPro;

[System.Serializable]
public struct FilterBackgroundMapping
{
    public RecipeFilterCategory category;
    public Texture textures;
}

/// <summary>
/// Controls the visual representation of a single Recipe Slot in the Recipe Book.
/// </summary>
public class RecipeSlotUI : MonoBehaviour, ISelectHandler, IDeselectHandler, ISlotUI<RecipeStatus>
{
    [Header("Dependency References")]
    [SerializeField] UIEffect uiEffect;
    [SerializeField] UIEffectTweener backImgPatternAnimation;

    [Header("UI References")]
    [SerializeField] private List<FilterBackgroundMapping> categoryTextures;
    [SerializeField] private Image backImg;
    [SerializeField] private GameObject maskOverlay;

    [Header("Unlock Mechanism")]
    [SerializeField] private UIEffectTweener unlockHoldAnimation;
    [SerializeField] private GameObject lockIndicator;  // MaskOverlay > lockIndicator
    [SerializeField] private GameObject mask;           // MaskOverlay > mask

    [Header("Lock Indicator Animator")]
    [SerializeField] private Image lockIconImage;
    [SerializeField] private Sprite[] lockFrames;
    [SerializeField] private float lockFrameRate = 10f;

    [Header("Inspiration cost")]
    [SerializeField] private GameObject inspirationCostGo;
    [SerializeField] private TMP_Text inspirationCostText;

    [Header("Icon Animator")]
    [SerializeField] public Image craftableIcon;

    [Header("Red Dot")]
    [SerializeField] private RedDotView redDotView;

    public RecipeStatus IData { get; private set; }
    public UIEffectTweener UnlockHoldAnimation => unlockHoldAnimation;

    // --- Unlock Visual Effects START ---
    private RectTransform _iconRectTransform;
    private Vector3 _iconDefaultScale;
    private Tween _iconScaleTween;
    private CanvasGroup _indicatorCanvasGroup;
    private Tween _iconLockIndicatorTween;
    // --- Unlock Visual Effects END ---

    // --- START: Used to solve the problem where the content
    // behind the maskOverlay can be briefly seen when switching tabs. 
    private CanvasGroup _backImgCanvasGroup;
    private Tween _backImgFadeTween;
    // --- END ---

    private Dictionary<RecipeFilterCategory, Texture> _textureDictionary;
    private Vector3 defaultIndicatorPos;
    private CanvasGroup _inspirationCg;

    private void Awake()
    {
        if (craftableIcon != null)
        {
            _iconRectTransform = craftableIcon.GetComponent<RectTransform>();
            _iconDefaultScale = _iconRectTransform.localScale;
        }

        if (backImg != null)
        {
            _backImgCanvasGroup = backImg.GetComponent<CanvasGroup>();
            if (_backImgCanvasGroup == null)
                _backImgCanvasGroup = _backImgCanvasGroup.gameObject.AddComponent<CanvasGroup>();
        }

        if (lockIndicator != null)
        {
            _indicatorCanvasGroup = lockIndicator.GetComponent<CanvasGroup>();
            if (_indicatorCanvasGroup == null)
            {
                _indicatorCanvasGroup = lockIndicator.AddComponent<CanvasGroup>();
            }
            defaultIndicatorPos = lockIndicator.transform.localPosition;
        }

        _textureDictionary = new Dictionary<RecipeFilterCategory, Texture>();
        if (categoryTextures != null)
        {
            foreach (var mapping in categoryTextures)
            {
                if (!_textureDictionary.ContainsKey(mapping.category))
                {
                    _textureDictionary.Add(mapping.category, mapping.textures);
                }
            }
        }

        if (inspirationCostGo != null)
        {
            _inspirationCg = inspirationCostGo.GetComponent<CanvasGroup>();
            if (_inspirationCg == null)
            {
                _inspirationCg = inspirationCostGo.AddComponent<CanvasGroup>();
            }
        }
    }

    public void Initialize(RecipeStatus newStatus)
    {
        if (_iconRectTransform != null)
        {
            _iconScaleTween?.Kill();
            _iconRectTransform.localScale = _iconDefaultScale;
        }

        _backImgFadeTween?.Kill();
        _iconLockIndicatorTween?.Kill();

        IData = newStatus;

        if (newStatus != null && newStatus.recipe != null)
        {
            if (redDotView != null)
            {
                string path = $"{RedDotPaths.Recipe}/{newStatus.recipe.recipeName}";
                redDotView.SetPath(path);
            }
        }

        bool hasRecipe = (newStatus != null && newStatus.recipe != null);

        if (!hasRecipe)
        {
            if (backImg != null) backImg.enabled = false;
            if (_backImgCanvasGroup != null) _backImgCanvasGroup.alpha = 0f;
            if (maskOverlay != null) maskOverlay.SetActive(false);

            if (backImgPatternAnimation != null) backImgPatternAnimation.enabled = false;
            if (unlockHoldAnimation != null) unlockHoldAnimation.gameObject.SetActive(false);

            if (craftableIcon != null)
            {
                craftableIcon.enabled = false;
                craftableIcon.sprite = null;
            }
            
            if (inspirationCostGo != null)
            {
                inspirationCostGo.SetActive(false);
            }
            return;
        }

        if (hasRecipe)
        {
            // Set background texture based on category
            Texture targetTexture = null;
            RecipeFilterCategory filter = newStatus.recipe.FilterCategory;

            if (_textureDictionary.TryGetValue(filter, out Texture categoryTexture) && categoryTexture != null)
            {
                targetTexture = categoryTexture;
            }
            if (targetTexture != null)
                uiEffect.transitionTexture = targetTexture;
        }

        if (lockIconImage != null) lockIconImage.sprite = lockFrames[0];
        if (backImg != null) backImg.enabled = true;

        bool isUnlocked = newStatus.isUnlocked;

        if (isUnlocked)
        {
            if (maskOverlay != null) maskOverlay.SetActive(false);
            if (_backImgCanvasGroup != null) _backImgCanvasGroup.alpha = 1f;
        }
        else
        {
            if (maskOverlay != null) maskOverlay.SetActive(true);
            if (lockIndicator != null) {
                lockIndicator.SetActive(true);
                lockIndicator.transform.localPosition = defaultIndicatorPos;
            } 
            if (_backImgCanvasGroup != null) _backImgCanvasGroup.alpha = 0f;
            if (_indicatorCanvasGroup != null) _indicatorCanvasGroup.alpha = 1f;
        }

        if (backImgPatternAnimation != null) backImgPatternAnimation.SetPause(true);

        if (craftableIcon != null)
        {
            craftableIcon.enabled = hasRecipe;
            if (hasRecipe)
            {
                craftableIcon.sprite = newStatus.recipe.result.item.sprite;
                if (isUnlocked)
                {
                    craftableIcon.color = Color.white;
                }
                else
                {
                    craftableIcon.color = Color.black;
                }
            }
        }

        if (inspirationCostText != null && IData.recipe != null)
        {
            inspirationCostText.text = "Cost: " + IData.recipe.inspirationCost.ToString();
        }

        if (_inspirationCg != null) _inspirationCg.alpha = 0f;

        if (unlockHoldAnimation != null)
        {
            if (isUnlocked)
            {
                unlockHoldAnimation.enabled = false;
                unlockHoldAnimation.gameObject.SetActive(false);
            }
            else
            {
                unlockHoldAnimation.gameObject.SetActive(true);
                unlockHoldAnimation.ResetTime(UIEffectTweener.Direction.Forward);
                unlockHoldAnimation.enabled = true; 
            }
        }
    }

    private void OnDestroy()
    {
        _iconScaleTween?.Kill();
        _backImgFadeTween?.Kill();
        _iconLockIndicatorTween?.Kill();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (IData != null && eventData != null && eventData.selectedObject)
        {
            Debug.Log("Select the recipe: " + IData.recipe.recipeName);
        }

        if (backImgPatternAnimation != null)
        {
            backImgPatternAnimation.Play();
        }

        // Show cost if locked
        if (_inspirationCg != null && IData != null && !IData.isUnlocked)
        {
            PlayShowInspirationText();
        }
        // TODO: StartCoroutine(HideNewTagAfterDelay(1.0f))
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (backImgPatternAnimation != null)
        {
            backImgPatternAnimation.SetPause(true);
        }

        if (_inspirationCg != null)
        {
            PlayHideInspirationText();
        }
    }

    #region UI Effect Tweener
    // Called by Controller when "Hold to Unlock" starts
    private void StartUnlockAnimation()
    {
        if (unlockHoldAnimation != null && unlockHoldAnimation.gameObject.activeInHierarchy)
        {
            unlockHoldAnimation.enabled = true;
            unlockHoldAnimation.ResetTime();
            unlockHoldAnimation.PlayForward(true);
        }

        if (_backImgCanvasGroup != null)
        {
            _backImgFadeTween?.Kill();
            _backImgFadeTween = _backImgCanvasGroup.DOFade(1f, 0.2f);
        }
    }

    private void CancelUnlockAnimationCoroutine()
    {
        if (unlockHoldAnimation != null && unlockHoldAnimation.gameObject.activeInHierarchy)
        {
            unlockHoldAnimation.enabled = true;
            unlockHoldAnimation.PlayReverse();
        }
    }
    #endregion

    #region UI Craftable Icon Tweener
    private void StartIconUnlockAnimation()
    {
        if (_iconRectTransform == null || unlockHoldAnimation == null) return;
        _iconScaleTween?.Kill();

        float duration = unlockHoldAnimation.duration;
        _iconScaleTween = _iconRectTransform.DOScale(_iconDefaultScale * 1.2f, duration).SetEase(Ease.OutQuad);
    }

    private void PlayIconUnlockSuccessAnimation()
    {
        if (_iconRectTransform == null) return;

        _backImgFadeTween?.Kill();
        _backImgCanvasGroup.alpha = 1f;
        _iconScaleTween?.Kill();

        _iconRectTransform.localScale = _iconDefaultScale;

        // "Pop" effect on success
        _iconScaleTween = DOTween.Sequence()
            .Append(_iconRectTransform.DOScale(_iconDefaultScale * 1.5f, 0.1f).SetEase(Ease.OutQuad))
            .Append(_iconRectTransform.DOScale(_iconDefaultScale, 0.3f).SetEase(Ease.OutBack))
            .Join(craftableIcon.DOColor(Color.white, 0.3f));
    }

    private void CancelIconUnlockAnimation()
    {
        if (_iconRectTransform == null) return;
        _iconScaleTween?.Kill();
        _iconScaleTween = _iconRectTransform.DOScale(_iconDefaultScale, 0.15f).SetEase(Ease.OutQuad);

        _backImgFadeTween?.Kill();
        _backImgFadeTween = _backImgCanvasGroup.DOFade(0f, 0.15f);
    }
    #endregion

    #region Indicator Icon Tweener
    public void PlayLockIndicatorAnimation()
    {
        if (lockFrames == null || lockIconImage == null) return;

        StartCoroutine(LockIndicatorCoroutine());

        _iconLockIndicatorTween?.Kill();

        // Move lock up and fade out
        _iconLockIndicatorTween = DOTween.Sequence()
            .Append(lockIndicator.transform.DOLocalMoveY(12f, 0.3f).SetRelative(true).SetEase(Ease.OutQuad))
            .Append(_indicatorCanvasGroup.DOFade(0, 1f));
    }

    private IEnumerator LockIndicatorCoroutine()
    {
        float frameDelay = 1f / lockFrameRate;

        for (int i = 0; i < lockFrames.Length; i++)
        {
            lockIconImage.sprite = lockFrames[i];
            yield return new WaitForSeconds(frameDelay);
        }
    }
    #endregion

    #region Inspiration UI
    private void PlayShowInspirationText()
    {
        _inspirationCg?.DOKill();

        _inspirationCg.DOFade(1, 0.2f);
    }

    private void PlayHideInspirationText()
    {
        _inspirationCg?.DOKill();

        _inspirationCg.DOFade(0, 0.2f);
    }
    #endregion

    #region Controller invocation
    // Public methods called by RecipeBookController
    public void StartUnlockVisuals()
    {
        StartUnlockAnimation();             // UI Effect Tweener
        StartIconUnlockAnimation();         // Icon Tweener
    }
    
    public void PlayUnlockSuccessVisuals()
    {
        // ResetUnlockAnimationOnSuccess();

        PlayLockIndicatorAnimation();
        PlayIconUnlockSuccessAnimation();
        PlayHideInspirationText();
    }

    public void CancelUnlockVisuals()
    {
        CancelIconUnlockAnimation();

        CancelUnlockAnimationCoroutine();
    }
    #endregion
}
