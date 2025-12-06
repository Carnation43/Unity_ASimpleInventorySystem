using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages real-time Gaussian Blur generation for UI elements
/// </summary>
public class BlurManager : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("The source RenderTexture to be blurred")]
    [SerializeField] private RenderTexture inputTexture; 
    [Tooltip("Blur Material")]
    [SerializeField] private Material blurMaterial; 

    [Header("Gaussian Blur Settings")]
    [Range(0, 5)]
    [SerializeField] private float blurSize = 1.0f;
    [Range(1, 16)]
    [SerializeField] private int iterations = 4;
    [Range(1, 4)]
    [SerializeField] private int downsample = 2;

    // use a dictionary for storing render texture
    private Dictionary<Recipe, RenderTexture> _blurCache = new Dictionary<Recipe, RenderTexture>();

    /// <summary>
    /// Try to get a texture from the cache
    /// </summary>
    public RenderTexture GetCachedTexture(Recipe recipe)
    {
        if (recipe != null && _blurCache.TryGetValue(recipe, out var texture))
        {
            return texture;
        }
        return null;
    }

    /// <summary>
    /// Generates a new blurred texture and caches it.
    /// </summary>
    public RenderTexture GenerateNewBlurredTexture(Recipe recipeToCache)
    {
        if (blurMaterial == null)
        {
            Debug.LogError("Blur Material is missing!");
            return null;
        }

        blurMaterial.SetFloat("_BlurSize", blurSize);

        // downsample the texture size
        int width = inputTexture.width / downsample;
        int height = inputTexture.height / downsample;

        // Get temporary textures for ping-pong blurring
        RenderTexture rt_temp1 = RenderTexture.GetTemporary(width, height, 0, inputTexture.format);
        RenderTexture rt_temp2 = RenderTexture.GetTemporary(width, height, 0, inputTexture.format);
        // Copy inputTexture to smaller rt_temp
        Graphics.Blit(inputTexture, rt_temp1);

        for (int i = 0; i < iterations; i++)
        {
            Graphics.Blit(rt_temp1, rt_temp2, blurMaterial, 0);
            Graphics.Blit(rt_temp2, rt_temp1, blurMaterial, 1);
        }

        // 3. Create a permanent texture for the cache 
        RenderTexture newCachedTexture = new RenderTexture(inputTexture.width, inputTexture.height, 0, inputTexture.format);
        newCachedTexture.Create();

        Graphics.Blit(rt_temp1, newCachedTexture);

        RenderTexture.ReleaseTemporary(rt_temp1);
        RenderTexture.ReleaseTemporary(rt_temp2);

        // 4. store in the cache
        if (recipeToCache != null)
        {
            _blurCache[recipeToCache] = newCachedTexture;
        }

        return newCachedTexture;
    }

    void OnDestroy()
    {
        foreach (var texture in _blurCache.Values)
        {
            if (texture != null)
            {
                texture.Release();
            }
        }
        _blurCache.Clear();
    }
}