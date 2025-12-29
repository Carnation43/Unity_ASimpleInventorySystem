using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InfiniteScrollTest
{
    public class TestInfiniteGridView : MonoBehaviour, IGridView
    {
        [Header("Core Settings")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private TestSlotUI _slotPrefab;

        [Header("Layout Settings")]
        [SerializeField] private int _columnCount = 1;
        [SerializeField] private Vector2 _cellSize = new Vector2(100, 100);
        [SerializeField] private Vector2 _spacing = new Vector2(10, 10);
        [SerializeField] private int _extraBufferLines = 2;

        private List<TestItemData> _dataList = new List<TestItemData>();
        private List<TestSlotUI> _activeSlots = new List<TestSlotUI>();

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

        private void Start()
        {
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.AddListener(OnScroll);
            }
        }

        public void Initialize(List<TestItemData> newData)
        {
            _dataList = newData;

            Canvas.ForceUpdateCanvases();

            // 1. Calculate Virtual Height
            // total rows = Ceil(Total / Column)
            int totalRows = Mathf.CeilToInt((float)_dataList.Count / _columnCount);
            float contentHeight = totalRows * (_cellSize.y + _spacing.y) - _spacing.y;

            // set virtual height to contain items
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, contentHeight);

            // 2. Instantiate required slots
            float viewportHeight = _scrollRect.viewport.rect.height;
            int visibleRows = Mathf.CeilToInt(viewportHeight / (_cellSize.y + _spacing.y));

            int totalSlotsNeeded = (visibleRows + _extraBufferLines) * _columnCount;

            RebuildSlots(totalSlotsNeeded);

            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, 0);
            _scrollRect.velocity = Vector2.zero;

            OnScroll(Vector2.zero);
        }

        public void RebuildSlots(int count)
        {
            foreach (var slot in _activeSlots)
            {
                if (slot != null) Destroy(slot.gameObject);
            }
            _activeSlots.Clear();

            for (int i = 0; i < count; i++)
            {
                TestSlotUI newSlot = Instantiate(_slotPrefab, _content);
                newSlot.gameObject.SetActive(false);
                _activeSlots.Add(newSlot);
            }
        }

        private void OnScroll(Vector2 pos)
        {
            if (_dataList.Count == 0 || _activeSlots.Count == 0) return;

            float scrollY = _content.anchoredPosition.y;
            if (scrollY < 0) scrollY = 0;

            int firstVisibleRow = Mathf.FloorToInt(scrollY / (_cellSize.y + _spacing.y));

            int firstDataIndex = firstVisibleRow * _columnCount;

            // Recycling Slots
            for (int i = 0; i < _activeSlots.Count; i++)
            {
                TestSlotUI slot = _activeSlots[i];

                int dataIndex = firstDataIndex + i;

                if (dataIndex < _dataList.Count)
                {
                    slot.gameObject.SetActive(true);

                    // refresh data
                    slot.Initialize(_dataList[dataIndex]);

                    int row = dataIndex / _columnCount;
                    int col = dataIndex % _columnCount;

                    float xPos = col * (_cellSize.x + _spacing.x);
                    float yPos = -row * (_cellSize.y + _spacing.y);

                    RectTransform rt = slot.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(xPos, yPos);
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }

        public int GetDataIndex(GameObject slotObj)
        {
            foreach (var slot in _activeSlots)
            {
                if (slot.gameObject == slotObj && slot.gameObject.activeSelf)
                {
                    return slot.IData.ID;
                }
            }
            return -1;
        }

        public void SelectDataIndex(int dataIndex)
        {
            Debug.Log($"Jump to {dataIndex}");
        }
    }
}
