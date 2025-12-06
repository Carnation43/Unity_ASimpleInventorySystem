using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic cinematic bars for specific actions
/// </summary>
public class CinematicBars : MonoBehaviour
{
    public static CinematicBars Instance { get; private set; }

    [Header("Components")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;

    [Header("Settings")]
    [Tooltip("The range occupied by the black borders")]
    [SerializeField] private float barRatio = 0.15f;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        topBar.sizeDelta = new Vector2(0, Screen.height * barRatio);
        bottomBar.sizeDelta = new Vector2(0, Screen.height * barRatio);

        topBar.anchoredPosition = new Vector2(0, topBar.rect.height);
        bottomBar.anchoredPosition = new Vector2(0, -bottomBar.rect.height);
    }

    public void ShowCinematicBars(float duration = 0.5f)
    {
        gameObject.SetActive(true);
        topBar.DOKill();
        bottomBar.DOKill();

        topBar.DOAnchorPosY(0, duration).SetEase(Ease.InOutSine);
        bottomBar.DOAnchorPosY(0, duration).SetEase(Ease.InOutSine);
    }

    public void HideCinematicBars(float duration = 0.5f)
    {
        gameObject.SetActive(true);
        topBar.DOKill();
        bottomBar.DOKill();

        topBar.DOAnchorPosY(topBar.rect.height, duration).SetEase(Ease.InOutSine);
        bottomBar.DOAnchorPosY(-bottomBar.rect.height, duration).SetEase(Ease.InOutSine)
            .OnComplete(() => gameObject.SetActive(false));
    }

    #region Debug Testing
    [ContextMenu("Show")]
    public void DebugShow()
    {
        ShowCinematicBars(1.5f);
    }

    [ContextMenu("Hide")]
    public void DebugHide()
    {
        HideCinematicBars(1.5f);
    }
    #endregion
}
