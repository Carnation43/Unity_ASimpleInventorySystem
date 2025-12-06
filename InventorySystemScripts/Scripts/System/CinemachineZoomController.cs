using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// game camera control script
/// </summary>
public class CinemachineZoomController : MonoBehaviour
{
    public static CinemachineZoomController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;

    [Header("Default Settings")]
    [SerializeField] private float _defaultZoomDuration = 1.0f;
    [SerializeField] private Ease _defaultEase = Ease.InOutSine;

    // cache component
    private CinemachineFramingTransposer _framingTransposer;

    // default copy
    private float _originalSize;
    private float _originalScreenX;
    private float _originalScreenY;

    private Tweener _zoomTween;
    private Tweener _screenXTween;
    private Tweener _screenYTween;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _framingTransposer = _virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (_virtualCamera != null)
        {
            _originalSize = _virtualCamera.m_Lens.OrthographicSize;
        }

        if (_framingTransposer != null)
        {
            _originalScreenX = _framingTransposer.m_ScreenX;
            _originalScreenY = _framingTransposer.m_ScreenY;
        }
    }

    public void ZoomTo(float targetSize, float duration = -1f, Ease? ease = null)
    {
        if (_virtualCamera == null) return;

        float finalDuration = duration < 0 ? _defaultZoomDuration : duration;
        Ease finalEase = ease ?? _defaultEase;

        _zoomTween?.Kill();

        _zoomTween = DOTween.To(
            () => _virtualCamera.m_Lens.OrthographicSize,
            x => { _virtualCamera.m_Lens.OrthographicSize = x; },
            targetSize,
            finalDuration
            )
            .SetEase(finalEase)
            .SetUpdate(true);
    }

    /// <summary>
    /// Adjust Camera's position and orthographic size in the meantime.
    /// </summary>
    /// <param name="targetSize">Orthographic size</param>
    /// <param name="targetScreenX">Target screen X (0-1), 0.5 is centered, less than 0.5 means the character is to the left, and greater than 0.5 means the character is to the right</param>
    /// <param name="targetScreenY">Target screen Y (0-1)</param>
    /// <param name="duration">Animation duration</param>
    /// <param name="ease">Ease function type</param>
    public void ZoomTo(float targetSize, float targetScreenX, float targetScreenY, float duration = -1f, Ease? ease = null)
    {
        if (_virtualCamera == null) return;

        float finalDuration = duration < 0 ? _defaultZoomDuration : duration;
        Ease finalEase = ease ?? _defaultEase;

        ZoomTo(targetSize, finalDuration, finalEase);

        _screenXTween?.Kill();

        _screenXTween = DOTween.To(
            () => _framingTransposer.m_ScreenX,
            x => _framingTransposer.m_ScreenX = x,
            targetScreenX,
            finalDuration
            )
            .SetEase(finalEase)
            .SetUpdate(true);

        _screenYTween?.Kill();

        _screenYTween = DOTween.To(
            () => _framingTransposer.m_ScreenY,
            x => _framingTransposer.m_ScreenY = x,
            targetScreenY,
            finalDuration
            )
            .SetEase(finalEase)
            .SetUpdate(true);
    }

    public void ResetZoom(float duration = -1f)
    {
        ZoomTo(_originalSize, _originalScreenX, _originalScreenY, duration);
    }

    public float GetOriginalSize() => _originalSize;
}
