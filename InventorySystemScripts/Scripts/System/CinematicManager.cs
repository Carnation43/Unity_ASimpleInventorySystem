using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic cinematic manager for specific actions
/// </summary>
public class CinematicManager : MonoBehaviour
{
    public static CinematicManager Instance { get; private set; }

    [Header("System References")]
    [Tooltip("Reference to the script controlling the top and bottom cinematic bars")]
    [SerializeField] private CinematicBars _cinematicBars;

    [Tooltip("Reference to the script controlling camera zoom via Cinemachine")]
    [SerializeField] private CinemachineZoomController _zoomController;

    [Tooltip("Reference to a full-screen black CanvasGroup for fade transition")]
    [SerializeField] private CanvasGroup _blackScreenFolder;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (_blackScreenFolder != null)
        {
            _blackScreenFolder.blocksRaycasts = false;
            _blackScreenFolder.alpha = 0f;
        }
    }

    public void ShowBars(float duration = 1.0f)
    {
        if (_cinematicBars != null)
        {
            _cinematicBars.ShowCinematicBars(duration);
        }
        else
        {
            Debug.LogWarning("[CinematicManager] CinematicBars reference is missing.");
        }
    }

    public void HideBars(float duration = 1.0f)
    {
        if (_cinematicBars != null)
        {
            _cinematicBars.HideCinematicBars(duration);
        }
        else
        {
            Debug.LogWarning("[CinematicManager] CinematicBars reference is missing.");
        }
    }

    /// <summary>
    /// Zooms the camera to a specific orthographic size
    /// </summary>
    /// <param name="targetSize">Target orthographic size</param>
    /// <param name="duration">Animation duration in seconds</param>
    public void ZoomTo(float targetSize, float duration = 1.0f)
    {
        if (_zoomController != null)
        {
            _zoomController.ZoomTo(targetSize, duration);
        }
        else
        {
            Debug.LogWarning("[CinematicManager] ZoomController reference is missing");
        }
    }

    public void MoveAndZoom(float targetSize, float screenX, float screenY, float duration = 1.0f)
    {
        if (_zoomController != null)
        {
            _zoomController.ZoomTo(targetSize, screenX, screenY, duration);
        }
        else
        {
            Debug.LogWarning("[CinematicManager] ZoomController reference is missing");
        }
    }

    /// <summary>
    /// Resets the camera to its original size
    /// </summary>
    public void ResetZoom(float duration = 1.0f)
    {
        if (_zoomController != null)
        {
            _zoomController.ResetZoom(duration);
        }
    }

    /// <summary>
    /// Fades the screen to black or transparent
    /// </summary>
    /// <param name="targetAlpha">1 for full black, 0 for transparent</param>
    /// <param name="duration">Animation duration in seconds</param>
    /// <returns>Coroutine for waiting until the fade is complete</returns>
    public IEnumerator FadeScreen(float targetAlpha, float duration)
    {
        if (_blackScreenFolder == null)
        {
            Debug.LogWarning("[CinematicManager] Black Scrren Fader is missing");
            yield break;
        }

        // Block raycasts if the screen is not fully transparent to prevent clicks
        _blackScreenFolder.blocksRaycasts = targetAlpha > 0.1f;

        yield return _blackScreenFolder.DOFade(targetAlpha, duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .WaitForCompletion();
    }

    #region Convience methods

    public void EnterCutsceneMode(float zoomSize = 7.0f, float duration = 1.0f)
    {
        ShowBars(duration);
        ZoomTo(zoomSize, duration);
    }

    public void ExitCutsceneMode(float duration = 1.0f)
    {
        HideBars(duration);
        ResetZoom(duration);
    }

    #endregion
}
