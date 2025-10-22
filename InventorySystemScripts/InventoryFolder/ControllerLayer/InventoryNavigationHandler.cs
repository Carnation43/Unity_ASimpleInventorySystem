using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryNavigationHandler : MonoBehaviour
{
    private MenuStateManager _stateManager;
    private InventoryView _viewController;
    private GridLayoutGroup _group;

    [Header("Settings")]
    [SerializeField] private float navigationCooldown = 0.1f;

    private Coroutine _navigationCoroutine;

    public void Initialize(MenuStateManager stateManager, InventoryView viewController, GridLayoutGroup group)
    {
        _stateManager = stateManager;
        _viewController = viewController;
        _group = group;
    }

    public void MoveOneStep(Vector2 move)
    {
        if (MenuController.instance == null || MenuController.instance.currentFocus != MenuController.MenuFocus.Inventory) return;

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

        if (newIndex >= 0 && newIndex < _viewController.SlotUIList.Count)
        {
            var targetSlot = _viewController.SlotUIList[newIndex];
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
            if (_stateManager.LastSelectedIndex == _viewController.SlotUIList.Count - 1) return 0;
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
            if (_stateManager.LastSelectedIndex + count.x >= _viewController.SlotUIList.Count) 
                return (_viewController.SlotUIList.Count - 1) - _stateManager.LastSelectedIndex;
            return count.x;
        }
    }

    /// <summary>
    /// (public method) check currently selected item is in the last index
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