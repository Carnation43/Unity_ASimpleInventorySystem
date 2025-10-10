using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// icon to display
/// </summary>
public class RadialPartUI : MonoBehaviour
{
    [SerializeField] public Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] public TMP_Text actionText;

    private CanvasGroup _iconCanvasGroup;
    private CanvasGroup _textCanvasGroup;

    private Sequence seq;

    private void Awake()
    {
        _iconCanvasGroup = iconImage.GetComponent<CanvasGroup>();
        _textCanvasGroup = actionText.GetComponent<CanvasGroup>();

        _textCanvasGroup.alpha = 0;
    }

    public void SetIcon(Sprite newIcon)
    {
        if(newIcon != null)
        {
            iconImage.sprite = newIcon;
        }
    }

    public void SetActionText(string newText)
    {
        if (newText != null)
        {
            actionText.text = newText;
        }
    }

    // temp
    public void UpdateVisuals(Color color, float fillAmount)
    {
        if(backgroundImage != null)
        {
            backgroundImage.color = color;
            backgroundImage.fillAmount = fillAmount;
        }
    }

    public void AnimateSelected()
    {
        seq?.Kill();

        seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector2.one * 1.2f, 0.2f).SetEase(Ease.OutBack));
        seq.Join(_iconCanvasGroup.DOFade(0, 0.2f));
        seq.Join(_textCanvasGroup.DOFade(1, 0.2f));
    }

    public void AnimateDeselected()
    {
        seq?.Kill();

        seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector2.one, 0.2f).SetEase(Ease.OutBack));
        seq.Join(_iconCanvasGroup.DOFade(1, 0.2f));
        seq.Join(_textCanvasGroup.DOFade(0, 0.2f));
    }
}
