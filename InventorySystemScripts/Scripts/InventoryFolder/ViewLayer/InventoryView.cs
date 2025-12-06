using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System;

/// <summary>
/// Manage the visual representation of the invetory grid.
/// It's responsible for creating, destroying, and updating content changes.
/// as well as handling the animations when the inventory content changes.
/// </summary>
public class InventoryView : BaseGridView<InventorySlot, InventorySlotUI>
{
    public static InventoryView instance;

    [Header("View References")]
    [SerializeField] private GameObject _overlay;            // show overlay when open the radial menu
    
    [Header("Dependencies")]
    [SerializeField] private RadialMenuModel _radialMenuModel;
    [SerializeField] private SelectionStateManager _menuStateManager;

    private Transform _originalParent;                       // Remember the original parent container
    private int _originalIndex;                              // Remember the original arrangement position
    private GameObject _currentSelected;
    private GameObject _placeholder;                         // placeholder object

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_radialMenuModel != null)
        {
            _radialMenuModel.OnMenuStateChanged += HandleRadialMenuStateChanged;
        }
    }

    private void OnDisable()
    {
        if (_radialMenuModel != null)
        {
            _radialMenuModel.OnMenuStateChanged -= HandleRadialMenuStateChanged;
        }

        if (_gridAnimator != null)
        {
            _gridAnimator.KillAllAnimations();
            _gridAnimator.ResetUI();
        }

        StopAllCoroutines();
    }

    /// <summary>
    /// The mask event is triggered when the radial menu is opened or closed.
    /// </summary>
    /// <param name="isOpen"></param>
    private void HandleRadialMenuStateChanged(bool isOpen)
    {
        if (_overlay == null) return;

        if (isOpen)
        {
            _overlay.SetActive(true);
            _currentSelected = _menuStateManager.LastItemSelected;

            if(_currentSelected != null)
            {
                _placeholder = new GameObject("InventorySlot_PlaceHolder");
                _placeholder.AddComponent<RectTransform>();
                _placeholder.transform.SetParent(_itemParentTransform, false);
                _placeholder.transform.SetSiblingIndex(_currentSelected.transform.GetSiblingIndex());

                _originalParent = _currentSelected.transform.parent;
                _originalIndex = _currentSelected.transform.GetSiblingIndex();

                // Move the position in the UI
                _currentSelected.transform.SetParent(_overlay.transform.parent, true);
                _currentSelected.transform.SetAsLastSibling();
            }
        }
        else
        {
            _overlay.SetActive(false);

            if(_currentSelected != null && _originalParent != null)
            {
                _currentSelected.transform.SetParent(_originalParent, true);
                _currentSelected.transform.SetSiblingIndex(_originalIndex);

                Destroy(_placeholder);
            }

            _currentSelected = null;
            _originalParent = null;
        }
    }

    // Specific logic to handle focus selection when an item stack is depleted
    public void SelectNewItemAfterConsume(int originalIndexOfConsumedItemInView)
    {
        StartCoroutine(SelectNewItemCoroutine(originalIndexOfConsumedItemInView));
    }

    private IEnumerator SelectNewItemCoroutine(int originalIndex)
    {
        // Wait for one frame to allow Unity's UI layout system to
        // finish destroying the old grids and rearranging the new grids.
        yield return new WaitForEndOfFrame();

        var activeItemSlots = _slotUIList
            .Where(s => s.gameObject.activeInHierarchy && s.IData != null && s.IData.item != null)
            .ToList();

        if (activeItemSlots.Count == 0)
        {
            // The tooltip will be automatically hidden when no item is selected.
            EventSystem.current.SetSelectedGameObject(null);
            yield break;
        }

        // When the item is the last one,
        // the Clamp function can ensure that
        // the new index does not go out of bounds
        // and remains the current last one.
        int newIndexToSelect = Mathf.Clamp(originalIndex, 0, activeItemSlots.Count - 1);

        // UI Component, data and new transform for obtaining the new item
        InventorySlotUI newSlotUI = activeItemSlots[newIndexToSelect];
        GameObject newObjectToSelect = newSlotUI.gameObject;

        // Reselect
        EventSystem.current.SetSelectedGameObject(newObjectToSelect);
    }

    #region [Deprecated method: AnimateAndRefreshCoroutine]
    /// <summary>
    /// A coroutine that manages the entire animation sequence:
    /// old items fade out, grid refreshes, and new items fade in.
    /// </summary>
    //private IEnumerator AnimateAndRefreshCoroutine(List<InventorySlot> items, bool resetSelection, int direction)
    //{
    //    // Get all currently visible slots to animate them out.
    //    var activeSlots = _slotUIList.Where(s => s.gameObject.activeInHierarchy).ToList();

    //    Sequence sequence = DOTween.Sequence();
    //    float moveDistance = 100;
    //    float duration = 0.2f;

    //    // Phase 1: Animate Old Items Out
    //    if (activeSlots.Any())
    //    {
    //        foreach (var slotUI in activeSlots)
    //        {
    //            // Ensure that slot has a CanvasGroup for fading
    //            CanvasGroup cg = slotUI.GetComponent<CanvasGroup>();
    //            if (cg == null) cg = slotUI.gameObject.AddComponent<CanvasGroup>();

    //            float startX = slotUI.transform.localPosition.x;
    //            sequence.Join(cg.DOFade(0, duration));
    //            sequence.Join(slotUI.transform.DOLocalMoveX(startX - direction * moveDistance, duration).SetEase(Ease.InBack));
    //        }
    //        yield return sequence.WaitForCompletion();
    //    }

    //    // Phase 2: Refresh the Grid Content
    //    RefreshInventoryGrid(items, resetSelection);

    //    // Wait for one frame, this allows Unity's layout system to calculate and apply the correct positions for the new slots. 
    //    yield return null;

    //    // Phase 3: Animatie New Items In
    //    var newActiveSlots = _slotUIList.Where(s => s.gameObject.activeInHierarchy).ToList();
    //    if (newActiveSlots.Any())
    //    {
    //        Sequence inSequence = DOTween.Sequence();
    //        foreach (var slotUI in newActiveSlots)
    //        {
    //            CanvasGroup cg = slotUI.GetComponent<CanvasGroup>() ?? slotUI.gameObject.AddComponent<CanvasGroup>();
    //            RectTransform rt = slotUI.GetComponent<RectTransform>();

    //            var finalPos = rt.anchoredPosition;

    //            rt.anchoredPosition = new Vector2(finalPos.x + direction * moveDistance, finalPos.y);
    //            cg.alpha = 0;

    //            inSequence.Join(cg.DOFade(1, duration));
    //            inSequence.Join(rt.DOAnchorPosX(finalPos.x, duration).SetEase(Ease.OutBack));
    //        }
    //    }
    //    _animationCoroutine = null;
    //}
    #endregion
}