using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// icon to display
/// </summary>
public class RadialPartUI : MonoBehaviour
{
    [SerializeField] public Image iconImage;
    [SerializeField] private Image backgroundImage;

    public void SetIcon(Sprite newIcon)
    {
        if(newIcon != null)
        {
            iconImage.sprite = newIcon;
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
}
