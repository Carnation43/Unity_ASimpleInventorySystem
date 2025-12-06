using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseGridView<T_SlotData, T_SlotUI> : MonoBehaviour, IGridView
    where T_SlotData : class
    where T_SlotUI : MonoBehaviour, ISlotUI<T_SlotData>
{ 

    [Header("Based Grid View References")]
    [Tooltip("SlotUI prefab")]
    [SerializeField] protected GameObject _itemSlotPrefab;

    [Tooltip("The parent object of the slot (ScrollRect -> Content)")]
    [SerializeField] protected Transform _itemParentTransform;

    [Tooltip("Extra empty slots")]
    [SerializeField] protected int _extraEmptySlots = 0;

    [Header("Base Grid Animation")]
    [Tooltip("Used for playing Tab switching animation logic")]
    [SerializeField] protected GridAnimator _gridAnimator;

    protected List<T_SlotUI> _slotUIList = new List<T_SlotUI>();
    public List<T_SlotUI> SlotUIList => _slotUIList;

    protected List<Selectable> _selectableSlotList = new List<Selectable>();

    public IReadOnlyList<Selectable> SelectableSlots => _selectableSlotList;

    protected Coroutine _selectFirstItemCoroutine;
    protected Coroutine _animationCoroutine;

    protected virtual void OnEnable()
    {
        if (_gridAnimator != null)
        {
            _gridAnimator.Initialize(_itemParentTransform);
        }
        else
        {
            Debug.LogError("[RecipeBookView] InventoryAnimator missing references");
        }
    }

    /// <summary>
    /// The public entry point for refreshing the inventory with an animation.
    /// It starts a coroutine to handle the animation sequence safely.
    /// </summary>
    /// <param name="items">The list of inventory data to display</param>
    /// <param name="resetSelection">whether to force the selection to the first item</param>
    /// <param name="direction">The direction of the tab switch(1 for right, -1 for left)</param>
    public void AnimateAndRefresh(List<T_SlotData> items, bool resetSelection, int direction)
    {
        // Stop any ongoing animation to prevent conflicts if the user presses or clicks too fast.
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }

        // When the direction is 0 or there are no items to display, skipping animation and refresing instantly.
        if (direction == 0 || !_slotUIList.Any(s => s.gameObject.activeInHierarchy))
        {
            RefreshGrid(items, resetSelection);

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

    private IEnumerator AnimateContainerAndRefreshCoroutine(List<T_SlotData> items, bool resetSelection, int direction)
    {
        // start animation -> refreshAction -> end animation
        Action refreshAction = () =>
        {
            RefreshGrid(items, resetSelection);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_itemParentTransform as RectTransform);
        };

        // start animation
        yield return StartCoroutine(_gridAnimator.PlaySwitchAnimation(direction, refreshAction));

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

    /// <summary>
    /// Instantly refreshes the gird withou any anmiation.
    /// </summary>
    public void RefreshGrid(List<T_SlotData> items, bool resetSelection)
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
            T_SlotUI newSlotUI = newSlotObj.GetComponent<T_SlotUI>();
            Selectable newSelectable = newSlotObj.GetComponent<Selectable>();

            if (newSlotUI != null && newSelectable != null)
            {
                _slotUIList.Add(newSlotObj.GetComponent<T_SlotUI>());
                _selectableSlotList.Add(newSelectable);
            }
            else
            {
                Destroy(newSlotObj);
                return;
            }
        }

        while (_slotUIList.Count > neededSlots)
        {
            int lastIndex = _slotUIList.Count - 1;
            Destroy(_slotUIList[lastIndex].gameObject);
            _slotUIList.RemoveAt(lastIndex);
            _selectableSlotList.RemoveAt(lastIndex);
        }

        int lastSelectedIndex = -1;
        if (!resetSelection && EventSystem.current.currentSelectedGameObject != null)
        {
            Selectable selectedSlotUI = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
            if (selectedSlotUI != null)
            {
                lastSelectedIndex = _selectableSlotList.IndexOf(selectedSlotUI);
            }
        }

        foreach (var slot in _slotUIList)
        {
            slot.gameObject.SetActive(false);
        }

        // Activate and initialize only the necessary slots
        for (int i = 0; i < items.Count; i++)
        {
            _slotUIList[i].Initialize(items[i]);
            _slotUIList[i].gameObject.SetActive(true);
        }

        for (int i = items.Count; i < neededSlots; i++)
        {
            _slotUIList[i].Initialize(null);
            _slotUIList[i].gameObject.SetActive(true);
        }

        if (!resetSelection && lastSelectedIndex != -1 && lastSelectedIndex < items.Count)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_selectableSlotList[lastSelectedIndex].gameObject);
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
            T_SlotUI firstActiveSlot = _slotUIList.FirstOrDefault(slot =>
                 slot.gameObject.activeInHierarchy &&
                 slot.IData != null &&
                 slot.GetComponent<Selectable>() != null &&
                 slot.GetComponent<Selectable>().interactable
             );

            if (firstActiveSlot != null)
            {
                EventSystem.current.SetSelectedGameObject(firstActiveSlot.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
