using DG.Tweening;
using InstanceResetToDefault;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controls a single button in the Side Menu.
/// Handles visual states(Selected, Pressed), animations (Breathing, Flash), and Audio
/// </summary>
public class SideMenuButton : MonoBehaviour, IResettableUI, ISelectHandler, IDeselectHandler
{
    // Notify SideMenuController
    public event Action<SideMenuButton> OnSelected;

    [Header("Configuration")]
    public SideMenuType menuType;

    [Header("UI References")]
    [Tooltip("Red notification dot")]
    [SerializeField] private GameObject redDot;
    [Tooltip("The out border image")]
    [SerializeField] private Image borderImage;
    [Tooltip("The inner glow background")]
    [SerializeField] private CanvasGroup backImgCanvasGroup;
    [Tooltip("The button text label")]
    [SerializeField] private TMP_Text btnText;

    [Header("Animation Settings")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color selectedTextColor = Color.white;
    [SerializeField] private float scaleMagnitude = 1.2f;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioCueEventChannel uiAudioChannel;
    [SerializeField] private AudioCueSO confirmCue;

    // Internal State
    private bool _isSelected;
    private Sequence _breathingSequence;
    private Vector3 normalScale = Vector3.one;

    private void Awake()
    {
        ResetUI();
    }

    private void OnDisable()
    {
        _breathingSequence?.Kill();
        ResetUI();
        _isSelected = false;
    }

    /// <summary>
    /// External API to toggle the red dot notifaction
    /// </summary>
    public void SetRedDot(bool isActive)
    {
        if (redDot != null) redDot.SetActive(isActive);
    }

    /// <summary>
    /// External API for SideMenuController
    /// </summary>
    public void OnConfirm()
    {
        if (uiAudioChannel != null && confirmCue != null)
        {
            uiAudioChannel.RaiseEvent(confirmCue);
        }
        PlayConfirmAnimation();
    }

    #region Event System Interfaces

    public void OnSelect(BaseEventData bed)
    {
        if (_isSelected) return;
        _isSelected = true;
        PlaySelectedAnimation();

        OnSelected?.Invoke(this);
    }

    public void OnDeselect(BaseEventData bed)
    {
        _isSelected = false;
        ReturnToNormalState();
    }

    #endregion

    #region Visual Logic

    private void PlaySelectedAnimation()
    {
        _breathingSequence?.Kill();
        _breathingSequence = DOTween.Sequence();

        if (btnText != null)
        {
            _breathingSequence.Join(btnText.DOColor(selectedTextColor, fadeDuration));
        }

        if (backImgCanvasGroup != null)
        {
            _breathingSequence.Join(backImgCanvasGroup.DOFade(1f, fadeDuration));
        }

        if (borderImage != null)
        {
            borderImage.rectTransform.localScale = normalScale;

            // Fade in first
            _breathingSequence.Join(borderImage.DOFade(1f, fadeDuration));

            // Then loop the alpha
            _breathingSequence.AppendCallback(() =>
            {
                borderImage.DOFade(0.1f, 0.75f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            });
        }
    }

    private void PlayConfirmAnimation()
    {
        borderImage?.DOKill();

        if (borderImage != null)
        {
            borderImage.color = new Color(borderImage.color.r, borderImage.color.g, borderImage.color.b, 1f);
            borderImage.rectTransform.DOScale(normalScale * scaleMagnitude, fadeDuration * 10).SetEase(Ease.OutQuint);
            borderImage.DOFade(0f, fadeDuration * 10).SetEase(Ease.OutQuint);
        }
    }

    private void ReturnToNormalState()
    {
        _breathingSequence?.Kill();
        borderImage?.DOKill();

        if (backImgCanvasGroup != null) backImgCanvasGroup.DOFade(0f, fadeDuration);
        if (btnText != null) btnText.DOColor(normalTextColor, fadeDuration);
        if (borderImage != null) borderImage.DOFade(0f, fadeDuration);
    }
    #endregion

    public void ResetUI()
    {
        if (backImgCanvasGroup != null) backImgCanvasGroup.alpha = 0f;
        if (redDot != null) redDot.SetActive(false);
        if (btnText != null) btnText.color = normalTextColor;
        if (borderImage != null)
        {
            borderImage.rectTransform.localScale = normalScale;
            Color c = borderImage.color;
            c.a = 0f;
            borderImage.color = c;
        }
    }

}