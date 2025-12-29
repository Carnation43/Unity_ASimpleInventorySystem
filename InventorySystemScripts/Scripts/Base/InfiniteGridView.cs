using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class InfiniteGridView<T_SlotData, T_SlotUI> : MonoBehaviour, IGridView
    where T_SlotData : class
    where T_SlotUI : MonoBehaviour, ISlotUI<T_SlotData>
{
    [Header("Core Settings")]
    [SerializeField] protected ScrollRect _scrollRect;
    [SerializeField] protected RectTransform _content;
    [SerializeField] protected T_SlotUI _slotPrefab;

    [Header("Layout Settings")]
    [SerializeField] protected int _columnCount = 5;
    [SerializeField] protected Vector2 _cellSize = new Vector2(95, 95);
    [SerializeField] protected Vector2 _spacing = new Vector2(0, 0);
    [SerializeField] protected int _extraBufferLines = 2;

    // Data Source
    protected List<T_SlotData> _dataList = new List<T_SlotData>();

    // Pool
    protected List<T_SlotUI> _activeSlots = new List<T_SlotUI>();

    // public data
    public IReadOnlyList<T_SlotUI> SlotUIList => _activeSlots;

    #region Implements interface
    public IReadOnlyList<Selectable> SelectableSlots
    {
        get
        {
            List<Selectable> list = new List<Selectable>();
            foreach (var slot in _activeSlots)
            {
                var sel = slot.GetComponent<Selectable>();
                if (sel) list.Add(sel);
            }
            return list;
        }
    }

    public int TotalDataCount => _dataList.Count;

    public int ColumnCount => _columnCount;

    public int GetDataIndex(GameObject slotObj)
    {
        for (int i = 0; i < _activeSlots.Count; i++)
        {
            if (_activeSlots[i].gameObject == slotObj && _activeSlots[i].gameObject.activeInHierarchy)
            {
                return _dataList.IndexOf(_activeSlots[i].IData);
            }
        }
        return -1;
    }

    /// <summary>
    /// 选中指定索引的数据（如果不可见，会自动滚过去）
    /// </summary>
    public void SelectDataIndex(int dataIndex)
    {
        if (dataIndex < 0 || dataIndex >= _dataList.Count) return;

        // 1. 计算目标位置
        int row = dataIndex / _columnCount;
        float targetY = row * (_cellSize.y + _spacing.y);

        float currentY = _content.anchoredPosition.y;
        float viewportHeight = _scrollRect.viewport.rect.height;

        // 2. 判断是否需要滚动
        // 如果目标在视口下方
        if (targetY > currentY + viewportHeight - _cellSize.y)
        {
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, targetY);
        }
        // 如果目标在视口上方
        else if (targetY < currentY)
        {
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, targetY);
        }

        // 3. 强制刷新
        OnScroll(Vector2.zero);
        Canvas.ForceUpdateCanvases();

        // 4. 选中对应的 UI
        foreach (var slot in _activeSlots)
        {
            if (slot.gameObject.activeInHierarchy && _dataList.IndexOf(slot.IData) == dataIndex)
            {
                EventSystem.current.SetSelectedGameObject(slot.gameObject);
                break;
            }
        }
    }

    #endregion

    protected virtual void Start()
    {
        if (_scrollRect != null)
        {
            _scrollRect.onValueChanged.AddListener(OnScroll);
        }
    }

    public virtual void Initialize(List<T_SlotData> newData)
    {
        _dataList = newData;

        // Canvas.ForceUpdateCanvases();

        // 1.Calculate the virtual content height
        int totalRows = Mathf.CeilToInt((float)_dataList.Count / _columnCount);
        float contentHeight = totalRows * (_cellSize.y + _spacing.y) - _spacing.y;

        if (contentHeight < 0) contentHeight = 0;

        _content.sizeDelta = new Vector2(_content.sizeDelta.x, contentHeight);

        // 2.Calculate the viewport height
        float viewportHeight = _scrollRect.viewport.rect.height;
        if (viewportHeight <= 0) viewportHeight = 500f;

        int visibleRows = Mathf.CeilToInt(viewportHeight / (_cellSize.y + _spacing.y));
        int totalSlotsNeeded = (visibleRows + _extraBufferLines) * _columnCount;

        // Rebuild slots pool
        RebuildSlots(totalSlotsNeeded);

        _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, 0);
        _scrollRect.velocity = Vector2.zero;

        OnScroll(Vector2.zero);
    }

    protected virtual void RebuildSlots(int count)
    {
        if (_activeSlots.Count > count)
        {
            for (int i = _activeSlots.Count - 1; i >= count; i--)
            {
                Destroy(_activeSlots[i].gameObject);
                _activeSlots.RemoveAt(i);
            }
        }

        while (_activeSlots.Count < count)
        {
            T_SlotUI newSlot = Instantiate(_slotPrefab, _content);
            newSlot.gameObject.SetActive(false);

            _activeSlots.Add(newSlot);
        }
    }

    protected virtual void OnScroll(Vector2 pos)
    {
        if (_dataList.Count == 0 || _activeSlots.Count == 0) return;

        float scrollY = _content.anchoredPosition.y;
        if (scrollY < 0) scrollY = 0;

        int firstVisibleRow = Mathf.FloorToInt(scrollY / (_cellSize.y + _spacing.y));

        int firstDataIndex = firstVisibleRow * _columnCount;

        // Recycling Slots
        for (int i = 0; i < _activeSlots.Count; i++)
        {
            T_SlotUI slot = _activeSlots[i];

            int dataIndex = firstDataIndex + i;

            if (dataIndex < _dataList.Count)
            {
                slot.gameObject.SetActive(true);

                // refresh data
                slot.Initialize(_dataList[dataIndex]);

                int row = dataIndex / _columnCount;
                int col = dataIndex % _columnCount;

                float xPos = col * (_cellSize.x + _spacing.x) + (_cellSize.x * 0.5f);
                float yPos = -row * (_cellSize.y + _spacing.y) - (_cellSize.y * 0.5f);

                RectTransform rt = slot.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(xPos, yPos);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
}
