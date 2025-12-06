/// Credit zero3growlithe
/// sourced from: http://forum.unity3d.com/threads/scripts-useful-4-6-scripts-collection.264161/page-2#post-2011648
/// 
/// --- MODIFIED VERSION ---
/// Modifications by Gemini to correctly handle first/last row snapping
/// for grid layouts, and to fix inverted scrolling logic at edges.
/// ------------------------

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
        protected GridLayoutGroup TargetGridLayoutGroup { get; set; } // <-- [MODIFIED] Added cache for Grid


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

        /// <summary>
        /// 获取当前选中项在其父节点(LayoutListGroup)中的同级索引
        /// </summary>
        protected int CurrentSelectedIndex
        {
            get
            {
                if (CurrentTargetRectTransform == null) return -1;
                if (CurrentTargetRectTransform.parent != LayoutListGroup) return -1;
                return CurrentTargetRectTransform.GetSiblingIndex();
            }
        }

        /// <summary>
        /// 获取 LayoutListGroup 中子项的总数
        /// </summary>
        protected int TotalSelectableItems
        {
            get
            {
                if (LayoutListGroup == null) return 0;
                return LayoutListGroup.childCount;
            }
        }

        /// <summary>
        /// [MODIFIED]
        /// 获取网格的列数 (用于垂直滚动) 或 行数 (用于水平滚动)
        /// </summary>
        protected int RowOrColumnCount
        {
            get
            {
                // 确保 TargetGridLayoutGroup 已被缓存
                if (TargetGridLayoutGroup == null && LayoutListGroup != null)
                {
                    TargetGridLayoutGroup = LayoutListGroup.GetComponent<GridLayoutGroup>();
                }

                if (TargetGridLayoutGroup == null) return 1;

                // 使用你项目中的 GridLayoutGroupHelper 扩展方法
                // (这个 cs 文件你之前上传过)
                Vector2Int size = TargetGridLayoutGroup.Size();

                if (scrollDirection == ScrollType.HORIZONTAL)
                {
                    // 水平滚动, 我们关心行数 (size.y)
                    return (size.y > 0) ? size.y : 1;
                }

                // 垂直滚动, 我们关心列数 (size.x)
                return (size.x > 0) ? size.x : 1;
            }
        }


        //*** METHODS - PUBLIC ***//


        //*** METHODS - PROTECTED ***//
        protected virtual void Awake()
        {
            TargetScrollRect = GetComponent<ScrollRect>();
            ScrollWindow = TargetScrollRect.GetComponent<RectTransform>();

            // [MODIFIED] 缓存 GridLayoutGroup
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
            // [MODIFIED] 确保 Grid 引用有效
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
            // update current selected rect transform
            if (CurrentSelectedGameObject != LastCheckedGameObject)
            {
                CurrentTargetRectTransform = (CurrentSelectedGameObject != null) ?
                    CurrentSelectedGameObject.GetComponent<RectTransform>() :
                    null;

                // unlock automatic scrolling
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
            // check main references
            bool referencesAreIncorrect = (TargetScrollRect == null || LayoutListGroup == null || ScrollWindow == null);

            if (referencesAreIncorrect == true || IsManualScrollingAvailable == true)
            {
                return;
            }

            RectTransform selection = CurrentTargetRectTransform;

            // check if scrolling is possible
            if (selection == null || selection.transform.parent != LayoutListGroup.transform)
            {
                return;
            }

            // depending on selected scroll direction move the scroll rect to selection
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
            // move the current scroll rect to correct position
            float selectionPosition = -selection.anchoredPosition.y - (selection.rect.height * (1 - selection.pivot.y));

            float elementHeight = selection.rect.height;
            float maskHeight = ScrollWindow.rect.height;
            float listAnchorPosition = LayoutListGroup.anchoredPosition.y;

            // get the element offset value depending on the cursor move direction
            float offlimitsValue = GetScrollOffset(selectionPosition, listAnchorPosition, elementHeight, maskHeight);

            // [MODIFIED] 修复了当 content 高度为0时除零的问题
            if (LayoutListGroup.rect.height > 0)
            {
                TargetScrollRect.verticalNormalizedPosition +=
                    (offlimitsValue / LayoutListGroup.rect.height) * Time.unscaledDeltaTime * scrollSpeed;
            }

            // 如果 Content 高度小于视口，直接设置为1 (顶部)
            if (LayoutListGroup.rect.height <= maskHeight)
            {
                TargetScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void UpdateHorizontalScrollPosition(RectTransform selection)
        {
            // move the current scroll rect to correct position
            float selectionPosition = -selection.anchoredPosition.x - (selection.rect.width * (1 - selection.pivot.x));

            float elementWidth = selection.rect.width;
            float maskWidth = ScrollWindow.rect.width;
            float listAnchorPosition = -LayoutListGroup.anchoredPosition.x;

            // get the element offset value depending on the cursor move direction
            // [MODIFIED] 注意这里的负号，它反转了 GetScrollOffset 的逻辑以适应水平滚动
            float offlimitsValue = -GetScrollOffset(selectionPosition, listAnchorPosition, elementWidth, maskWidth);

            // [MODIFIED] 修复了当 content 宽度为0时除零的问题
            if (LayoutListGroup.rect.width > 0)
            {
                TargetScrollRect.horizontalNormalizedPosition +=
                    (offlimitsValue / LayoutListGroup.rect.width) * Time.unscaledDeltaTime * scrollSpeed;
            }

            // 如果 Content 宽度小于视口，直接设置为0 (左侧)
            if (LayoutListGroup.rect.width <= maskWidth)
            {
                TargetScrollRect.horizontalNormalizedPosition = 0f;
            }
        }

        // --- [MODIFIED] 这是被完全重写的核心方法 ---
        private float GetScrollOffset(float position, float listAnchorPosition, float targetLength, float maskLength)
        {
            // 垂直滚动: VNP=1 是顶部, VNP=0 是底部
            // GetScrollOffset 返回正值时，VNP增加 (向上滚动)
            // GetScrollOffset 返回负值时，VNP减少 (向下滚动)

            // 水平滚动: HNP=0 是左侧, HNP=1 是右侧
            // GetScrollOffset 返回正值 -> 被-号反转 -> HNP减少 (向左滚动)
            // GetScrollOffset 返回负值 -> 被-号反转 -> HNP增加 (向右滚动)

            float viewStart = listAnchorPosition;   // 视口顶/左侧 (Content的Y/X锚点位置)
            float viewEnd = listAnchorPosition + maskLength;     // 视口底/右侧
            float itemStart = position;             // 选中项顶/左侧
            float itemEnd = position + targetLength;        // 选中项底/右侧

            int index = CurrentSelectedIndex;
            if (index < 0) return 0; // 未选中

            int total = TotalSelectableItems;
            if (total == 0) return 0;

            int spanCount = RowOrColumnCount; // 获取列数（垂直）或行数（水平）
            if (spanCount == 0) spanCount = 1; // 防止除零

            // --- 边缘情况处理 ---

            // 1. 检查是否在第一行/第一列 (index < spanCount)
            int currentRowIndex = index / spanCount;
            if (currentRowIndex == 0)
            {
                // 目标：滚动到顶部/最左侧 (listAnchorPosition = 0)
                // (viewStart - itemStart) -> (listAnchorPosition - 0)
                // 返回 listAnchorPosition，它是一个正值 (e.g. 50)，
                // VNP 会增加 (向上滚)，直到 listAnchorPosition 变为 0
                return listAnchorPosition;
            }

            // 2. 检查是否在最后一行/最后一列
            int lastRowIndex = (total - 1) / spanCount;
            if (currentRowIndex == lastRowIndex)
            {
                // 目标：滚动到Gird的底部/最右侧
                float contentSize = (scrollDirection == ScrollType.VERTICAL || scrollDirection == ScrollType.BOTH) ? LayoutListGroup.rect.height : LayoutListGroup.rect.width;

                if (contentSize > maskLength)
                {
                    // 目标 (viewBottom == contentSize)
                    // (viewBottom - itemBottom) -> ( (listAnchorPosition + maskLength) - contentSize )
                    // 假设 contentSize=1000, maskLength=200, 目标 listAnchorPosition=800
                    // 假设当前 listAnchorPosition=750, viewEnd=950
                    // (950 - 1000) = -50
                    // VNP 会减少 (向下滚)，直到 listAnchorPosition 变为 800
                    return (listAnchorPosition + maskLength) - contentSize;
                }
                else
                {
                    // Content 小于视口，也应该滚回顶部
                    return listAnchorPosition;
                }
            }

            // --- 标准滚动逻辑 (非边缘行/列) ---
            if (itemStart < viewStart)
            {
                // 物品在视口 "前" (上/左)，向上/左滚动
                return viewStart - itemStart; // (返回正值)
            }
            else if (itemEnd > viewEnd)
            {
                // 物品在视口 "后" (下/右)，向下/右滚动
                return viewEnd - itemEnd; // (返回负值)
            }

            return 0; // 在视图内
        }
        // --- 方法替换结束 ---


        //*** ENUMS ***//
        public enum ScrollType
        {
            VERTICAL,
            HORIZONTAL,
            BOTH
        }
    }
}