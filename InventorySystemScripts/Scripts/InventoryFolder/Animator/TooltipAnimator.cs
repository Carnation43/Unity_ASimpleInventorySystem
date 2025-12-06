using DG.Tweening;
using InstanceResetToDefault;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Control the show and hide animation
/// </summary>
[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
public class TooltipAnimator : MonoBehaviour, IResettableUI
{
    private CanvasGroup _canvasGroup;
    private RectTransform _tooltipRectTransform;
    private Sequence _currentSeq;
    private Animator _btnDetails;
    private Animator _btnConfirm;
    private Tweener _detailsHoldTween;

    public bool IsAnimating { get; private set; }
    public bool IsHidden { get; private set; } = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _tooltipRectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Button detailsButton, Button equipButton)
    {
        if (detailsButton != null) _btnDetails = detailsButton.GetComponent<Animator>();
        if (equipButton != null) _btnConfirm = equipButton.GetComponent<Animator>();
    }

    public void TriggerConfirmAnimation() { if (_btnConfirm != null) _btnConfirm.SetTrigger("Pressed"); }
    public void TriggerDetailsAnimation() { if (_btnDetails != null) _btnDetails.SetTrigger("Pressed"); }

    public void HandleDetailsHoldAnimation(InputAction.CallbackContext context)
    {
        if (_btnDetails == null) return;
        Transform buttonTransform = _btnDetails.transform;
        const float holdTime = 0.3f;

        if (context.started)
        {
            _detailsHoldTween?.Kill(); 
            _detailsHoldTween = buttonTransform.DOScale(1.1f, holdTime).SetEase(Ease.OutQuad);
        }

        if (context.performed || context.canceled)
        {
            Debug.Log("Tooltip Hold Cancel");
            _detailsHoldTween?.Kill();
            _detailsHoldTween = buttonTransform.DOScale(1.0f, 0.1f);
        }
    }

    /// <summary>
    /// Simple Animation: Control navigation in the inventory
    /// </summary>
    public void Show(RectTransform _rectTransform)
    {
        if (_rectTransform == null) return;
        if (IsHidden) return;
        if (_canvasGroup.alpha < 1f)
        {
            _tooltipRectTransform.position = _rectTransform.position;
            _canvasGroup.DOFade(1, 0.15f);
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }

    public void Hide()
    {
        _canvasGroup.DOFade(0, 0.15f);
    }

    private void HideFull()
    {
        if (IsAnimating) return;
        IsAnimating = true;
        IsHidden = true;

        _currentSeq?.Kill();

        _currentSeq = DOTween.Sequence();
        _currentSeq.Append(_canvasGroup.DOFade(0, 0.15f));
        _currentSeq.Join(_tooltipRectTransform.DOScaleY(0, 0.5f).SetEase(Ease.InQuad));
        _currentSeq.OnComplete(() => { IsAnimating = false; });
    }

    public void ToggleTooltip(RectTransform _trackedRectTransform)
    {
        if (_trackedRectTransform == null) return;
        if (IsHidden) ShowFull(_trackedRectTransform);
        else HideFull();
    }

    private void ShowFull(RectTransform _trackedRectTransform)
    {
        if (IsAnimating) return;
        IsAnimating = true;
        IsHidden = false;

        _currentSeq?.Kill();

        _tooltipRectTransform.localScale = new Vector3(1, 0, 1);
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        _tooltipRectTransform.position = _trackedRectTransform.position;

        _currentSeq = DOTween.Sequence();
        _currentSeq.Append(_canvasGroup.DOFade(1, 0.15f));
        _currentSeq.Join(_tooltipRectTransform.DOScaleY(1, 0.5f).SetEase(Ease.OutQuad));
        _currentSeq.OnComplete(() =>
        {
            IsAnimating = false;
        });

    }

    public void ResetUI()
    {
        _currentSeq?.Kill();
        _detailsHoldTween?.Kill();

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        IsAnimating = false;
    }
}
