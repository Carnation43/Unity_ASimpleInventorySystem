// 文件: ClickDebugger.cs (或 WorldClickHandler.cs)

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic; // 需要引入这个命名空间

public class ClickDebugger : MonoBehaviour
{
    [Header("监听的输入频道")]
    [SerializeField] private InputEventChannel inputChannel;

    private Camera mainCamera;
    private bool _clickScheduled = false;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnAttack += ScheduleClickCheck;
        }
    }

    private void OnDisable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnAttack -= ScheduleClickCheck;
        }
    }

    private void ScheduleClickCheck(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _clickScheduled = true;
        }
    }

    private void Update()
    {
        if (!_clickScheduled)
        {
            return;
        }
        _clickScheduled = false;

        // ---- 这是修改的核心 ----
        // 我们不再使用 IsPointerOverGameObject()
        // 而是创建一个新的、更精确的检查，只判断是否点击了真正的UI
        if (IsPointerOverGenuineUI())
        {
            return;
        }
        // -------------------------

        // 执行射线检测
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log($"<color=lime>点击击中了: {hit.collider.gameObject.name}</color>");
            Ghost ghost = hit.collider.GetComponent<Ghost>();
            if (ghost != null)
            {
                ghost.BeClicked();
            }
        }
        else
        {
            Debug.Log("<color=orange>点击未击中任何物体。</color>");
        }
    }

    /// <summary>
    /// 精确检查鼠标指针是否悬停在真正的UI元素上
    /// </summary>
    private bool IsPointerOverGenuineUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // 真正的UI元素一定在Canvas下，并且其Layer通常是"UI"
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return true;
            }
        }
        return false;
    }
}