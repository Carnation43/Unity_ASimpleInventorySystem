using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// A generic component that handles grid navigation for any view immplementing IGridView
public class GridNavigationHandler : MonoBehaviour
{
    // Using generic references instead of specific "InventoryView"
    private SelectionStateManager _stateManager;
    private IGridView _gridView;
    private GridLayoutGroup _group;

    [Header("Listening To")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("Settings")]
    [SerializeField] private float navigationCooldown = 0.1f;

    private Coroutine _navigationCoroutine;

    private void OnEnable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnGlobalInputLock += HandleGlobalLock;
            inputChannel.OnConfirmStarted += HandleInterruptInput;
            inputChannel.OnRadialMenuHoldStart += HandleInterruptSignal;
        }
    }

    private void OnDisable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnGlobalInputLock -= HandleGlobalLock;
            inputChannel.OnConfirmStarted -= HandleInterruptInput;
            inputChannel.OnRadialMenuHoldStart -= HandleInterruptSignal;
        }
    }

    // Stop navigation if the system gets locked
    private void HandleGlobalLock(bool isLocked)
    {
        if (isLocked) StopContinuousNavigation();
    }

    // Stop navigation if the player starts an action
    private void HandleInterruptInput(InputAction.CallbackContext context)
    {
        StopContinuousNavigation();
    }

    private void HandleInterruptSignal()
    {
        StopContinuousNavigation();
    }

    public void Initialize(SelectionStateManager stateManager, IGridView gridView, GridLayoutGroup group)
    {
        _stateManager = stateManager;
        _gridView = gridView;
        _group = group;
    }

    public void MoveOneStep(Vector2 move)
    {
        int addition = 0;

        if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
        {
            if (move.x > 0.5f) addition = CalculateXAddition(1);
            else if (move.x < -0.5f) addition = CalculateXAddition(-1);
        }
        else
        {
            if (move.y > 0.5f) addition = CalculateYAddition(1);
            else if (move.y < -0.5f) addition = CalculateYAddition(-1);
        }

        if (addition != 0)
        {
            HandleNextItemSelection(addition);
        }
    }

    public void StartContinuousNavigation(Vector2 move)
    {
        if (_navigationCoroutine != null)
        {
            StopCoroutine(_navigationCoroutine);
        }
        _navigationCoroutine = StartCoroutine(ContinuousNavigationCoroutine(move));
    }

    public void StopContinuousNavigation()
    {
        if (_navigationCoroutine != null)
        {
            StopCoroutine(_navigationCoroutine);
            _navigationCoroutine = null;
        }
    }

    private IEnumerator ContinuousNavigationCoroutine(Vector2 move)
    {
        yield return new WaitForSecondsRealtime(0.3f);

        while (true)
        {
            MoveOneStep(move);
            yield return new WaitForSecondsRealtime(navigationCooldown);
        }
    }

    //public void ProcessNavigation(Vector2 move)
    //{
    //    if (_stateManager.LastItemSelected == null) return;

    //    if (move == Vector2.zero) return;
    //    if (Time.unscaledTime - _lastNavigationTime < navigationCooldown)
    //    {
    //        return; 
    //    }
    //    int addition = 0;

    //    if (move.x > 0.5f) addition = CalculateXAddition(1);
    //    else if (move.x < -0.5f) addition = CalculateXAddition(-1);
    //    else if (move.y > 0.5f) addition = CalculateYAddition(1);
    //    else if (move.y < -0.5f) addition = CalculateYAddition(-1);

    //    if (addition != 0)
    //    {
    //        HandleNextItemSelection(addition);
    //        _lastNavigationTime = Time.unscaledTime;
    //    }
    //}

    private void HandleNextItemSelection(int addition)
    {
        int newIndex = _stateManager.LastSelectedIndex + addition;

        if (newIndex >= 0 && newIndex < _gridView.SelectableSlots.Count)
        {
            var targetSlot = _gridView.SelectableSlots[newIndex];
            if (targetSlot.gameObject.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(targetSlot.gameObject);
            }
        }
    }

    private int CalculateXAddition(int direction)
    {
        Vector2Int count = GridLayoutGroupHelper.Size(_group);
        if (count.x <= 1) return 0;

        if (direction > 0) // right
        {
            if (_stateManager.LastSelectedIndex == _gridView.SelectableSlots.Count - 1) return 0;
            // if (_stateManager.LastSelectedIndex % count.x == count.x - 1) return 0;
            return 1;
        }
        else // left
        {
            // if (_stateManager.LastSelectedIndex % count.x == 0) return 0;
            return -1;
        }
    }

    private int CalculateYAddition(int direction)
    {
        Vector2Int count = GridLayoutGroupHelper.Size(_group);
        if (count.x == 0) return 0;

        if (direction > 0) // up
        {
            if (_stateManager.LastSelectedIndex - count.x < 0) return 0;
            return -count.x;
        }
        else // down
        {
            if (_stateManager.LastSelectedIndex + count.x >= _gridView.SelectableSlots.Count)
                return (_gridView.SelectableSlots.Count - 1) - _stateManager.LastSelectedIndex;
            return count.x;
        }
    }

    /// <summary>
    /// (public method) check currently selected item is in the last index
    /// Used by controllers to determine if navigation should jump to another panel
    /// </summary>
    /// <returns>bool value</returns>
    public bool IsOnLastRow()
    {
        if (_stateManager.LastItemSelected == null) return false;

        var gridLayoutSize = GridLayoutGroupHelper.Size(_group);
        if (gridLayoutSize.x == 0 || gridLayoutSize.y == 0) return false;

        int currentRowIndex = _stateManager.LastSelectedIndex / gridLayoutSize.x;

        int lastRowIndex = gridLayoutSize.y - 1;

        return currentRowIndex == lastRowIndex;
    }
}