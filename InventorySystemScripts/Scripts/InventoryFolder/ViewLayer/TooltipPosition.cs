using UnityEngine;
using DG.Tweening;

/// <summary>
/// Used to track and update the position of the tooltip
/// </summary>
public class TooltipPosition : MonoBehaviour
{
   
    [Tooltip("Calculate the position of the tooltip relative to the inventory")]
    [SerializeField] RectTransform scrollAreaRect;

    [Header("Dependencies")]
    [SerializeField] private GridAnimator gridAnimator;

    private RectTransform _tooltipRectTransform;
    public RectTransform _trackedRectTransform;

    private void Awake()
    {
        _tooltipRectTransform = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        _trackedRectTransform = null;
    }

    private void Update()
    {
         if (_trackedRectTransform != null)
        {
            if (gridAnimator != null && gridAnimator.IsAnimating)
            {
                _tooltipRectTransform.position = _trackedRectTransform.position;
            }
            else
            {
                _tooltipRectTransform.position = Vector3.Lerp(_tooltipRectTransform.position, _trackedRectTransform.position, Time.deltaTime * 15f);
            }
        }
        CalculatePivotPosition();
    }

    private void CalculatePivotPosition()
    {
        Vector2 localPos;
        Vector2 screenPoint;

        // Convert the tooltip's world coordinates to screen coordinates
        screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, _tooltipRectTransform.position);

        // Convert the tooltip's screen coordinates to the scrollArea's local coordinates
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            scrollAreaRect,
            screenPoint,
            Camera.main,
            out localPos
            );

        // Debug.Log("The local position of the Tooltip relative to the ScrollArea:" + localPos);

        Vector2 tooltipSize = _tooltipRectTransform.sizeDelta;

        float rightEdgeDelta = localPos.x + tooltipSize.x / 2;
        // Debug.Log("rightEdgeDelta: " + rightEdgeDelta);
        bool flipX = rightEdgeDelta > scrollAreaRect.sizeDelta.x / 2;

        // Debug.Log("flipX = " + flipX);

        float bottomEdgeDelta = -localPos.y + tooltipSize.y / 2;
        // Debug.Log("bottomEdgeDelta: " + bottomEdgeDelta);
        // Debug.Log("scrollAreaRect.sizeDelta.y: " + scrollAreaRect.sizeDelta.y / 2);
        bool flipY = bottomEdgeDelta > scrollAreaRect.sizeDelta.y / 2;

        // Debug.Log("flipY = " + flipY);
        Vector2 targetPivot = new Vector2(flipX ? 1 : 0, flipY ? 0 : 1);

        // Only set pivot if it actually changes to avoid unnecessary dirty layout flags.
        if (_tooltipRectTransform.pivot != targetPivot)
        {
            _tooltipRectTransform.pivot = targetPivot;
        }
    }
}
