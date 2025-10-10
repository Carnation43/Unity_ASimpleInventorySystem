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
public class InventoryView : MonoBehaviour
{
    public static InventoryView instance;

    [Header("View References")]
    [SerializeField] private GameObject _overlay;            // show overlay when open the radial menu
    [SerializeField] private GameObject _itemSlotPrefab;
    [SerializeField] private Transform _itemParentTransform; // the parent transform where all item slots will be instantiated
    [SerializeField] private int _extraEmptySlots = 5;

    public Transform ItemParentTransform => _itemParentTransform;

    // a list that holds all the UI slot components currently in the grid
    private List<InventorySlotUI> _slotUIList = new List<InventorySlotUI>();
    public List<InventorySlotUI> SlotUIList => _slotUIList;

    [Header("Animation")]
    [SerializeField] private InventoryAnimator _inventoryAnimator;

    [Header("Dependencies")]
    [SerializeField] private RadialMenuModel _radialMenuModel;
    [SerializeField] private MenuStateManager _menuStateManager;

    private Transform _originalParent;                       // Remember the original parent container
    private int _originalIndex;                              // Remember the original arrangement position
    private GameObject _currentSelected;
    private GameObject _placeholder;                         // placeholder object

    private Coroutine _selectFirstItemCoroutine;
    private Coroutine _animationCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
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
    }

    private void Start()
    {
        _inventoryAnimator.Initialize(_itemParentTransform);
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

    /// <summary>
    /// The public entry point for refreshing the inventory with an animation.
    /// It starts a coroutine to handle the animation sequence safely.
    /// </summary>
    /// <param name="items">The list of inventory data to displat</param>
    /// <param name="resetSelection">whether to force the selection to the first item</param>
    /// <param name="direction">The direction of the tab switch(1 for right, -1 for left)</param>
    public void AnimateAndRefresh(List<InventorySlot> items, bool resetSelection, int direction)
    {
        // Stop any ongoing animation to prevent conflicts if the user presses or clicks too fast.
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }

        // When the direction is 0 or there are no items to display, skipping animation and refresing instantly.
        if (direction == 0 || !_slotUIList.Any(s => s.gameObject.activeInHierarchy))
        {
            RefreshInventoryGrid(items, resetSelection);
            if (resetSelection)
            {
                if (_selectFirstItemCoroutine != null) StopCoroutine(_selectFirstItemCoroutine);
                _selectFirstItemCoroutine = StartCoroutine(SelectFirstItemAfterDelay(true));
            }
            return;
        }

        // Start the animation as a coroutine
        _animationCoroutine = StartCoroutine(AnimateContainerAndRefreshCoroutine(items, resetSelection, direction));
    }

    private IEnumerator AnimateContainerAndRefreshCoroutine(List<InventorySlot> items, bool resetSelection, int direction)
    {
        // start animation -> refreshAction -> end animation
        Action refreshAction = () =>
        {
            RefreshInventoryGrid(items, resetSelection);
          
            LayoutRebuilder.ForceRebuildLayoutImmediate(_itemParentTransform as RectTransform);
        };

        // start animation
        yield return StartCoroutine(_inventoryAnimator.PlaySwitchAnimation(direction, refreshAction));

        if (resetSelection)
        {
            if (_selectFirstItemCoroutine != null)
            {
                StopCoroutine(_selectFirstItemCoroutine);
            }

            // Start a coroutine to select the first item after a delay
            _selectFirstItemCoroutine = StartCoroutine(SelectFirstItemAfterDelay(resetSelection));
        }
        _animationCoroutine = null;
    }

    #region [Deprecated method: AnimateAndRefreshCoroutine]
    /// <summary>
    /// A coroutine that manages the entire animation sequence:
    /// old items fade out, grid refreshes, and new items fade in.
    /// </summary>
    private IEnumerator AnimateAndRefreshCoroutine(List<InventorySlot> items, bool resetSelection, int direction)
    {
        // Get all currently visible slots to animate them out.
        var activeSlots = _slotUIList.Where(s => s.gameObject.activeInHierarchy).ToList();

        Sequence sequence = DOTween.Sequence();
        float moveDistance = 100;
        float duration = 0.2f;

        // Phase 1: Animate Old Items Out
        if (activeSlots.Any())
        {
            foreach (var slotUI in activeSlots)
            {
                // Ensure that slot has a CanvasGroup for fading
                CanvasGroup cg = slotUI.GetComponent<CanvasGroup>();
                if (cg == null) cg = slotUI.gameObject.AddComponent<CanvasGroup>();

                float startX = slotUI.transform.localPosition.x;
                sequence.Join(cg.DOFade(0, duration));
                sequence.Join(slotUI.transform.DOLocalMoveX(startX - direction * moveDistance, duration).SetEase(Ease.InBack));
            }
            yield return sequence.WaitForCompletion();
        }

        // Phase 2: Refresh the Grid Content
        RefreshInventoryGrid(items, resetSelection);

        // Wait for one frame, this allows Unity's layout system to calculate and apply the correct positions for the new slots. 
        yield return null;

        // Phase 3: Animatie New Items In
        var newActiveSlots = _slotUIList.Where(s => s.gameObject.activeInHierarchy).ToList();
        if (newActiveSlots.Any())
        {
            Sequence inSequence = DOTween.Sequence();
            foreach (var slotUI in newActiveSlots)
            {
                CanvasGroup cg = slotUI.GetComponent<CanvasGroup>() ?? slotUI.gameObject.AddComponent<CanvasGroup>();
                RectTransform rt = slotUI.GetComponent<RectTransform>();
                
                var finalPos = rt.anchoredPosition;
       
                rt.anchoredPosition = new Vector2(finalPos.x + direction * moveDistance, finalPos.y);
                cg.alpha = 0;

                inSequence.Join(cg.DOFade(1, duration));
                inSequence.Join(rt.DOAnchorPosX(finalPos.x, duration).SetEase(Ease.OutBack));
            }
        }
        _animationCoroutine = null; 
    }
    #endregion

    /// <summary>
    /// Instantly refreshes the inventory gird withou any anmiation.
    /// </summary>
    public void RefreshInventoryGrid(List<InventorySlot> items, bool resetSelection)
    {
        if (_itemSlotPrefab == null || _itemParentTransform == null)
        {
            Debug.LogError("InventoryView references are missing!");
            return;
        }

        int neededSlots = items.Count + _extraEmptySlots;

        while (_slotUIList.Count < neededSlots)
        {
            GameObject newSlotObj = Instantiate(_itemSlotPrefab, _itemParentTransform);
            _slotUIList.Add(newSlotObj.GetComponent<InventorySlotUI>());
        }

        while (_slotUIList.Count > neededSlots)
        {
            int lastIndex = _slotUIList.Count - 1;
            Destroy(_slotUIList[lastIndex].gameObject);
            _slotUIList.RemoveAt(lastIndex);
        }

        int lastSelectedIndex = -1;
        if (!resetSelection && EventSystem.current.currentSelectedGameObject != null)
        {
            var selectedSlotUI = EventSystem.current.currentSelectedGameObject.GetComponent<InventorySlotUI>();
            if (selectedSlotUI != null)
            {
                lastSelectedIndex = _slotUIList.IndexOf(selectedSlotUI);
            }
        }

        foreach (var slot in _slotUIList)
        {
            slot.gameObject.SetActive(false);
        }

        // Activate and initialize only the necessary slots
        for (int i = 0; i < items.Count; i++)
        {
            _slotUIList[i].gameObject.SetActive(true);
            _slotUIList[i].Initialize(items[i]);
        }

        for (int i = items.Count; i < neededSlots; i++)
        {
            _slotUIList[i].gameObject.SetActive(true);
            _slotUIList[i].Initialize(null);
        }

        for (int i = items.Count; i < neededSlots; i++)
        {
            _slotUIList[i].gameObject.SetActive(true);
            _slotUIList[i].Initialize(null);
        }
        if (!resetSelection && lastSelectedIndex != -1 && lastSelectedIndex < items.Count)
        {
            EventSystem.current.SetSelectedGameObject(_slotUIList[lastSelectedIndex].gameObject);
        }
    }

    public IEnumerator SelectFirstItemAfterDelay(bool forceReset)
    {

        yield return null;

        if (!forceReset && EventSystem.current.currentSelectedGameObject != null)
        {
            yield break;
        }

        EventSystem.current.SetSelectedGameObject(null);

        if (_slotUIList.Count > 0)
        {
            InventorySlotUI firstActiveSlot = null;
            foreach (var slot in _slotUIList)
            {
                if (slot.gameObject.activeInHierarchy && slot.GetComponent<Selectable>() != null && slot.GetComponent<Selectable>().interactable)
                {
                    firstActiveSlot = slot;
                    break;
                }
            }

            if (firstActiveSlot != null)
            {
                EventSystem.current.SetSelectedGameObject(firstActiveSlot.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
                TooltipViewController.instance.HideTooltip();
            }
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            TooltipViewController.instance.HideTooltip();
        }
    }

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
            .Where(s => s.gameObject.activeInHierarchy && s.slot != null && s.slot.item != null)
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
        InventorySlot newSlotData = newSlotUI.slot;
        RectTransform newIconTransform = newSlotUI.icon.rectTransform;

        // force updating new contents before selecting the new item
        if (newSlotData != null && newIconTransform != null)
        {
            TooltipViewController.instance.ShowTooltip(newSlotData, newIconTransform);
        }

        // Reselect
        EventSystem.current.SetSelectedGameObject(newObjectToSelect);
    }
}