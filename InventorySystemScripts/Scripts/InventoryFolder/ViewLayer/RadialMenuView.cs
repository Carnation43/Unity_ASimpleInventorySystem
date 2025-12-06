using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the visual representation of the entire radial menu
/// Subscribing to events from the Model
/// Spawning, positioning, and destroying the individual menu slices.
/// Updating the visual state of the slices based on Model updates.
/// Handling the overall visibility and layout of the menu.
/// </summary>
public class RadialMenuView : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float angleBetweenPart = 10f;
    [SerializeField] private float centerDeadZoneRadius = 20f;
    [SerializeField] private float actionZoneRadius = 150f;
    [SerializeField] private float radius = 52f;

    [Header("References")]
    [SerializeField] private GameObject radialPartPrefab;
    [SerializeField] public Transform radialPartTransform;

    private RadialMenuModel _model;
    private List<RadialPartUI> _spawnParts = new List<RadialPartUI>();
    private CanvasGroup _canvasGroup;
    private float _currentFillAmount;

    public float CenterDeadZoneRadius => centerDeadZoneRadius;
    public float ActionZoneRadius => actionZoneRadius;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(RadialMenuModel model)
    {
        _model = model;
        _model.OnMenuStateChanged += HandleMenuStateChanged;
        _model.OnHighlightIndexChanged += HandleHighlightChanged;
    }

    private void OnDestroy()
    {
        if (_model != null)
        {
            _model.OnMenuStateChanged -= HandleMenuStateChanged;
            _model.OnHighlightIndexChanged -= HandleHighlightChanged;
        }
    }

    private void HandleMenuStateChanged(bool isOpen)
    {
        if (isOpen)
        {
            SpawnRadialParts();
            _canvasGroup.alpha = 1f; 
            _canvasGroup.interactable = true; 
            _canvasGroup.blocksRaycasts = true; 
        }
        else
        {
            // The actual destruction is handled by the animator.
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    private void SpawnRadialParts()
    {
        DestroyRadialParts();

        int numberOfRadialPart = _model.currentItems.Count;

        // Calculate the fill amount for each slice's background image.
        _currentFillAmount = (1f / numberOfRadialPart) - (angleBetweenPart / 360f);

        for (int i = 0; i < numberOfRadialPart; i++)
        {
            GameObject spawnRadialPart = Instantiate(radialPartPrefab, radialPartTransform);

            float angle = - i * 360 / numberOfRadialPart - angleBetweenPart / 2;
            Vector3 radialPartEularAngle = new Vector3(0, 0, angle);
            spawnRadialPart.transform.position = radialPartTransform.position;
            spawnRadialPart.transform.localEulerAngles = radialPartEularAngle;

            RadialPartUI partUI = spawnRadialPart.GetComponent<RadialPartUI>();
            if(partUI != null)
            {
                partUI.SetIcon(_model.currentItems[i].icon);
                partUI.SetActionText(_model.currentItems[i].actionName);
                CalculateIconPos(numberOfRadialPart, partUI, angle);
                partUI.UpdateVisuals(Color.white, _currentFillAmount);

                _spawnParts.Add(partUI);
            }

        }
    }

    private void CalculateIconPos(int numberOfRadialPart, RadialPartUI partUI, float angle)
    {
        // 1.Calculate the angle of each menu item (actual display + gap)
        float anglePerSlice = 360f / numberOfRadialPart;
        // 2.The actual angle size of the sector
        float arcWidth = anglePerSlice - angleBetweenPart;
        // 3. Half the angle of the sector (the angle that the icon should display)
        // Note: The parent object has already been rotated by half the gap angle;
        // this is the angle that the icon needs to rotate relative to the parent object
        float angleRelative = -arcWidth / 2f;
        // 4.Convert angles to radians
        float angleRelativeRad = angleRelative * Mathf.Deg2Rad;

        // 5.Convert polar coordinates to Cartesian coordinates
        // A negative angle indicates a clockwise rotation.
        float x = Mathf.Sin(-angleRelativeRad) * radius;
        // cos(x) = cos(-x) x¡Ê[0, PI]
        float y = Mathf.Cos(angleRelativeRad) * radius;
        partUI.iconImage.rectTransform.anchoredPosition = new Vector3(x, y, 0);
        partUI.actionText.rectTransform.anchoredPosition = new Vector3(x, y, 0);

        // Offset the icon rotation caused by the parent object's rotation;
        // you can delete it if you don't want it.
        partUI.iconImage.rectTransform.localEulerAngles = new Vector3(0, 0, -angle);
        partUI.actionText.rectTransform.localEulerAngles = new Vector3(0, 0, -angle);
    }

    private void DestroyRadialParts()
    {
        foreach (var item in _spawnParts)
        {
            Destroy(item.gameObject);
        }

        _spawnParts.Clear();
    }

    private void HandleHighlightChanged(int newIndex)
    {
        for (int i = 0; i < _spawnParts.Count; i++)
        {
            bool isSelected = (i == newIndex);

            _spawnParts[i].UpdateVisuals(Color.white, _currentFillAmount);

            if (isSelected)
            {
                _spawnParts[i].AnimateSelected();
            }
            else
            {
                _spawnParts[i].AnimateDeselected();
            }
        }
    }

    /// <summary>
    /// Hand over to Animator for invocation
    /// </summary>
    public void DestroyMenuAndParts()
    {
        DestroyRadialParts();
        _canvasGroup.alpha = 0f;
    }
}
