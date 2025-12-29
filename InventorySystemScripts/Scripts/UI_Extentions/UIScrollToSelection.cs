using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
    [RequireComponent(typeof(ScrollRect))]
    [AddComponentMenu("UI/Extensions/UIScrollToSelection")]
    public class UIScrollToSelection : MonoBehaviour
    {
        //*** ATTRIBUTES ***//
        [Header("[ Settings ]")]
        [SerializeField]
        private ScrollType scrollDirection = ScrollType.BOTH;
        [SerializeField]
        private float scrollSpeed = 10f;

        [Header("[ Buffer ]")]
        [SerializeField]
        [Tooltip("滚动时预留的边缘空隙，确保物品不会贴边显示。建议设置为格子间距的一半 (例如 20-50)。")]
        private float scrollBuffer = 20f; // 默认给一个合理的值

        [Header("[ Input ]")]
        [SerializeField]
        private bool cancelScrollOnInput = false;
        [SerializeField]
        private List<KeyCode> cancelScrollKeycodes = new List<KeyCode>();

        //*** PROPERTIES ***//
        // REFERENCES
        protected RectTransform LayoutListGroup
        {
            get { return TargetScrollRect != null ? TargetScrollRect.content : null; }
        }

        // SETTINGS
        protected ScrollType ScrollDirection
        {
            get { return scrollDirection; }
        }
        protected float ScrollSpeed
        {
            get { return scrollSpeed; }
        }

        // INPUT
        protected bool CancelScrollOnInput
        {
            get { return cancelScrollOnInput; }
        }
        protected List<KeyCode> CancelScrollKeycodes
        {
            get { return cancelScrollKeycodes; }
        }

        // CACHED REFERENCES
        protected RectTransform ScrollWindow { get; set; }
        protected ScrollRect TargetScrollRect { get; set; }
        protected GridLayoutGroup TargetGridLayoutGroup { get; set; }


        // SCROLLING
        protected EventSystem CurrentEventSystem
        {
            get { return EventSystem.current; }
        }
        protected GameObject LastCheckedGameObject { get; set; }
        protected GameObject CurrentSelectedGameObject
        {
            get { return EventSystem.current.currentSelectedGameObject; }
        }
        protected RectTransform CurrentTargetRectTransform { get; set; }
        protected bool IsManualScrollingAvailable { get; set; }

        protected int CurrentSelectedIndex
        {
            get
            {
                if (CurrentTargetRectTransform == null) return -1;
                if (CurrentTargetRectTransform.parent != LayoutListGroup) return -1;
                return CurrentTargetRectTransform.GetSiblingIndex();
            }
        }

        protected int TotalSelectableItems
        {
            get
            {
                if (LayoutListGroup == null) return 0;
                return LayoutListGroup.childCount;
            }
        }

        protected int RowOrColumnCount
        {
            get
            {
                if (TargetGridLayoutGroup == null && LayoutListGroup != null)
                {
                    TargetGridLayoutGroup = LayoutListGroup.GetComponent<GridLayoutGroup>();
                }

                if (TargetGridLayoutGroup == null) return 1;

                // 使用你的 GridLayoutGroupHelper 扩展
                Vector2Int size = TargetGridLayoutGroup.Size();

                if (scrollDirection == ScrollType.HORIZONTAL)
                {
                    return (size.y > 0) ? size.y : 1;
                }
                return (size.x > 0) ? size.x : 1;
            }
        }

        //*** METHODS - PROTECTED ***//
        protected virtual void Awake()
        {
            TargetScrollRect = GetComponent<ScrollRect>();
            ScrollWindow = TargetScrollRect.GetComponent<RectTransform>();

            if (LayoutListGroup != null)
            {
                TargetGridLayoutGroup = LayoutListGroup.GetComponent<GridLayoutGroup>();
            }
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {
            if (TargetGridLayoutGroup == null && LayoutListGroup != null)
            {
                TargetGridLayoutGroup = LayoutListGroup.GetComponent<GridLayoutGroup>();
            }

            UpdateReferences();
            CheckIfScrollingShouldBeLocked();
            ScrollRectToLevelSelection();
        }

        //*** METHODS - PRIVATE ***//
        private void UpdateReferences()
        {
            if (CurrentSelectedGameObject != LastCheckedGameObject)
            {
                CurrentTargetRectTransform = (CurrentSelectedGameObject != null) ?
                    CurrentSelectedGameObject.GetComponent<RectTransform>() :
                    null;

                if (CurrentSelectedGameObject != null &&
                    CurrentSelectedGameObject.transform.parent == LayoutListGroup.transform)
                {
                    IsManualScrollingAvailable = false;
                }
            }

            LastCheckedGameObject = CurrentSelectedGameObject;
        }

        private void CheckIfScrollingShouldBeLocked()
        {
            if (CancelScrollOnInput == false || IsManualScrollingAvailable == true)
            {
                return;
            }

            for (int i = 0; i < CancelScrollKeycodes.Count; i++)
            {
                if (Input.GetKeyDown(CancelScrollKeycodes[i]) == true)
                {
                    IsManualScrollingAvailable = true;
                    break;
                }
            }
        }

        private void ScrollRectToLevelSelection()
        {
            bool referencesAreIncorrect = (TargetScrollRect == null || LayoutListGroup == null || ScrollWindow == null);

            if (referencesAreIncorrect == true || IsManualScrollingAvailable == true)
            {
                return;
            }

            RectTransform selection = CurrentTargetRectTransform;

            if (selection == null || selection.transform.parent != LayoutListGroup.transform)
            {
                return;
            }

            switch (ScrollDirection)
            {
                case ScrollType.VERTICAL:
                    UpdateVerticalScrollPosition(selection);
                    break;
                case ScrollType.HORIZONTAL:
                    UpdateHorizontalScrollPosition(selection);
                    break;
                case ScrollType.BOTH:
                    UpdateVerticalScrollPosition(selection);
                    UpdateHorizontalScrollPosition(selection);
                    break;
            }
        }

        private void UpdateVerticalScrollPosition(RectTransform selection)
        {
            float selectionPosition = -selection.anchoredPosition.y - (selection.rect.height * (1 - selection.pivot.y));

            float elementHeight = selection.rect.height;
            float maskHeight = ScrollWindow.rect.height;
            float listAnchorPosition = LayoutListGroup.anchoredPosition.y;

            float offlimitsValue = GetScrollOffset(selectionPosition, listAnchorPosition, elementHeight, maskHeight);

            if (LayoutListGroup.rect.height > 0)
            {
                TargetScrollRect.verticalNormalizedPosition +=
                    (offlimitsValue / LayoutListGroup.rect.height) * Time.unscaledDeltaTime * scrollSpeed;
            }

            if (LayoutListGroup.rect.height <= maskHeight)
            {
                TargetScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void UpdateHorizontalScrollPosition(RectTransform selection)
        {
            float selectionPosition = -selection.anchoredPosition.x - (selection.rect.width * (1 - selection.pivot.x));

            float elementWidth = selection.rect.width;
            float maskWidth = ScrollWindow.rect.width;
            float listAnchorPosition = -LayoutListGroup.anchoredPosition.x;

            float offlimitsValue = -GetScrollOffset(selectionPosition, listAnchorPosition, elementWidth, maskWidth);

            if (LayoutListGroup.rect.width > 0)
            {
                TargetScrollRect.horizontalNormalizedPosition +=
                    (offlimitsValue / LayoutListGroup.rect.width) * Time.unscaledDeltaTime * scrollSpeed;
            }

            if (LayoutListGroup.rect.width <= maskWidth)
            {
                TargetScrollRect.horizontalNormalizedPosition = 0f;
            }
        }

        // --- 核心修改部分 ---
        private float GetScrollOffset(float position, float listAnchorPosition, float targetLength, float maskLength)
        {
            float viewStart = listAnchorPosition;
            float viewEnd = listAnchorPosition + maskLength;
            float itemStart = position;
            float itemEnd = position + targetLength;

            int index = CurrentSelectedIndex;
            if (index < 0) return 0;

            int total = TotalSelectableItems;
            if (total == 0) return 0;

            int spanCount = RowOrColumnCount;
            if (spanCount == 0) spanCount = 1;

            // 1. 检查是否在第一行/第一列
            // 如果是第一个，我们强制对齐顶部，不加 buffer，避免顶部出现奇怪的留白
            int currentRowIndex = index / spanCount;
            if (currentRowIndex == 0)
            {
                return listAnchorPosition;
            }

            // 2. 检查是否在最后一行/最后一列
            // 如果是最后一个，我们强制对齐底部，不加 buffer
            int lastRowIndex = (total - 1) / spanCount;
            if (currentRowIndex == lastRowIndex)
            {
                float contentSize = (scrollDirection == ScrollType.VERTICAL || scrollDirection == ScrollType.BOTH) ? LayoutListGroup.rect.height : LayoutListGroup.rect.width;

                if (contentSize > maskLength)
                {
                    return (listAnchorPosition + maskLength) - contentSize;
                }
                else
                {
                    return listAnchorPosition;
                }
            }

            // 3. 正常滚动逻辑 (加入 Buffer 判定)

            // 目标：物品不仅要进入 ViewStart，还要进入 ViewStart + Buffer 的区域
            // 如果 itemStart < viewStart + scrollBuffer，说明物品太靠上/左了（或者被切掉了）
            if (itemStart < viewStart + scrollBuffer)
            {
                // 我们希望 itemStart 最终位于 viewStart + scrollBuffer 的位置
                // 差值 = (viewStart) - (itemStart - scrollBuffer) = viewStart - itemStart + scrollBuffer
                // 返回正值 -> Scroll Up / Left
                return viewStart - itemStart + scrollBuffer;
            }

            // 目标：物品不仅要进入 ViewEnd，还要进入 ViewEnd - Buffer 的区域
            // 如果 itemEnd > viewEnd - scrollBuffer，说明物品太靠下/右了（或者被切掉了）
            else if (itemEnd > viewEnd - scrollBuffer)
            {
                // 我们希望 itemEnd 最终位于 viewEnd - scrollBuffer 的位置
                // 差值 = (viewEnd) - (itemEnd + scrollBuffer) = viewEnd - itemEnd - scrollBuffer
                // 返回负值 -> Scroll Down / Right
                return viewEnd - itemEnd - scrollBuffer;
            }

            return 0;
        }

        public enum ScrollType
        {
            VERTICAL,
            HORIZONTAL,
            BOTH
        }
    }
}