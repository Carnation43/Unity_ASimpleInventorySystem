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

    public void Initialize(SelectionStateManager stateManager, IGridView gridView)
    {
        _stateManager = stateManager;
        _gridView = gridView;
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
            _gridView.SelectDataIndex(newIndex);
        }
    }

    private int CalculateXAddition(int direction)
    {
        int columnCount = _gridView.ColumnCount;
        int totalCount = _gridView.TotalDataCount;
        int currentIndex = GetCurrentDataIndex();

        if (columnCount <= 1 && totalCount > 1) columnCount = 1;

        if (direction > 0) // right
        {
            if (currentIndex >= totalCount - 1) return 0;
            // if (_stateManager.LastSelectedIndex % count.x == count.x - 1) return 0;
            return 1;
        }
        else // left
        {
            // if (_stateManager.LastSelectedIndex % count.x == 0) return 0;
            if (currentIndex <= 0) return 0;
            return -1;
        }
    }

    private int CalculateYAddition(int direction)
    {
        int columnCount = _gridView.ColumnCount;
        int totalCount = _gridView.TotalDataCount;
        int currentIndex = GetCurrentDataIndex();

        if (columnCount == 0) return 0;

        if (direction > 0) // up
        {
            if (currentIndex - columnCount < 0) return 0;
            return -columnCount;
        }
        else // down
        {
            if (currentIndex + columnCount < totalCount)
                return columnCount;
            if (currentIndex < totalCount - 1)
                return (totalCount - 1) - currentIndex;

            return 0;
        }
    }

    private int GetCurrentDataIndex()
    {
        if (_stateManager.LastItemSelected == null) return 0;
        int index = _gridView.GetDataIndex(_stateManager.LastItemSelected);
        return (index == -1) ? 0 : index;
    }

    /// <summary>
    /// (public method) check currently selected item is in the last index
    /// Used by controllers to determine if navigation should jump to another panel
    /// </summary>
    /// <returns>bool value</returns>
    public bool IsOnLastRow()
    {
        int currentIndex = GetCurrentDataIndex();
        int totalCount = _gridView.TotalDataCount;
        int columnCount = _gridView.ColumnCount;

        if (columnCount == 0) return false;

        int currentRow = currentIndex / columnCount;
        int lastRow = (totalCount - 1) / columnCount;

        return currentRow == lastRow;
    }
}