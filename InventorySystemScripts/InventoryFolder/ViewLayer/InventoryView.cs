using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    public static InventoryView instance;

    [Header("View References")]
    [SerializeField] private GameObject _itemSlotPrefab;
    [SerializeField] private Transform _itemParentTransform;
    [SerializeField] private int _extraEmptySlots = 5;

    private List<InventorySlotUI> _slotUIList = new List<InventorySlotUI>();
    public List<InventorySlotUI> SlotUIList => _slotUIList;

    private Coroutine _selectFirstItemCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void RefreshInventoryGrid(List<InventorySlot> items)
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

        if (_selectFirstItemCoroutine != null)
        {
            StopCoroutine(_selectFirstItemCoroutine);
        }

        // Start a coroutine to select the first item after a delay
        _selectFirstItemCoroutine = StartCoroutine(SelectFirstItemAfterDelay());
    }

    private IEnumerator SelectFirstItemAfterDelay()
    {
        yield return null;

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
                TooltipInstance.instance.Hide();
            }
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            TooltipInstance.instance.Hide();
        }
    }
}