using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Specifically handles the "Blur Overlay" effect by interacting with BlurManager.
/// </summary>
public class RecipePanelLogic : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject contentUnlocked; // The actual content panel
    [SerializeField] private GameObject contentLocked;   // The overlay image shown when locked
    [Tooltip("The RawImage component on the Locked Overlay that displays the blurred texture")]
    [SerializeField] private RawImage outputRawImage;

    [Header("Dependencies")]
    [SerializeField] private BlurManager blurManager;    // Reference to the blur generator
    [SerializeField] private Camera blurCamera;         

    private Coroutine _lockCoroutine;
    // used for unlocking 
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = contentLocked.gameObject.GetComponent<CanvasGroup>();
        if (_canvasGroup == null) contentLocked.gameObject.AddComponent<CanvasGroup>();
    }

    // Locked a recipe
    public void LockRecipe(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("recipe is null");
            return;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 1f;
        }

        if (_lockCoroutine != null)
        {
            StopCoroutine(_lockCoroutine);
        }
        _lockCoroutine = StartCoroutine(LockRecipeCoroutine(recipe));
    }

    // Check cache first
    private IEnumerator LockRecipeCoroutine(Recipe recipe)
    {
        // If we have already generated a blur for this recipe, use it instantly.
        RenderTexture cachedTexture = blurManager.GetCachedTexture(recipe);

        if (cachedTexture != null)
        {
            // cahce hit
            Debug.Log($"Cache hit: {recipe.name}");
            outputRawImage.texture = cachedTexture; 
            contentUnlocked.SetActive(false);
            contentLocked.SetActive(true);

            _lockCoroutine = null;
            yield break;
        }

        // Cache miss
        // Active blur Camera for one frame to capture the scene
        if (blurCamera != null)
            blurCamera.gameObject.SetActive(true);

        yield return null; 

        // Generate and cache
        RenderTexture newBlur = blurManager.GenerateNewBlurredTexture(recipe);

        if (outputRawImage != null)
        {
            outputRawImage.texture = newBlur;
        }

        // disabled blur camera
        if (blurCamera != null)
            blurCamera.gameObject.SetActive(false);

        // switch UI
        if (contentUnlocked != null) contentUnlocked.SetActive(false);
        if (contentLocked != null) contentLocked.SetActive(true);

        _lockCoroutine = null;
    }

    public void UnlockRecipe()
    {
        if (_lockCoroutine != null)
        {
            StopCoroutine(_lockCoroutine);
            _lockCoroutine = null;
            if (blurCamera != null)
                blurCamera.gameObject.SetActive(false);
        }

        if (contentUnlocked != null) contentUnlocked.SetActive(true);

        if (_canvasGroup != null)
        {
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0, 1f).SetEase(Ease.InOutSine).OnComplete(() => { contentLocked.SetActive(false); });
        }
    }

    /// <summary>
    /// Display immediately
    /// </summary>
    public void ShowUnlockedDetails()
    {
        if (_lockCoroutine != null)
        {
            StopCoroutine(_lockCoroutine);
            _lockCoroutine = null;
            if (blurCamera != null)
                blurCamera.gameObject.SetActive(false);
        }

        if (contentUnlocked != null) contentUnlocked.SetActive(true);
        if (contentLocked != null) contentLocked.SetActive(false);

    }
}