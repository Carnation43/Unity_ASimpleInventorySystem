using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttributeRowUI : MonoBehaviour
{
    [Header("UI Reference")]
    public Image selectedImage;
    public TMP_Text currentLevelText;
    public Image leftArrow;
    public Image rightArrow;
    public TMP_Text nextLevelText;

    [Tooltip("Display left and right upgrade prompts after selection")]
    public GameObject selectedIndicators;
}
