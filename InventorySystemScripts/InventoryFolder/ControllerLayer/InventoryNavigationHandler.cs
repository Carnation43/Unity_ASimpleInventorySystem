using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryNavigationHandler : MonoBehaviour
{
    private MenuStateManager _stateManager;
    private InventoryView _viewController;
    private GridLayoutGroup _group;

    public void Initialize(MenuStateManager stateManager, InventoryView viewController, GridLayoutGroup group)
    {
        _stateManager = stateManager;
        _viewController = viewController;
        _group = group;
    }

    public void HandleNavigationInput()
    {
        if (_stateManager.LastItemSelected == null) return;

        Vector2 move = UserInput.UIMoveInput;
        if (move == Vector2.zero) return;

        int addition = 0;

        if (move.x > 0.5f) addition = CalculateXAddition(1);
        else if (move.x < -0.5f) addition = CalculateXAddition(-1);
        else if (move.y > 0.5f) addition = CalculateYAddition(1);
        else if (move.y < -0.5f) addition = CalculateYAddition(-1);

        if (addition != 0)
        {
            HandleNextItemSelection(addition);
        }
    }

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
            if (_stateManager.LastSelectedIndex % count.x == count.x - 1) return 0;
            return 1;
        }
        else // left
        {
            if (_stateManager.LastSelectedIndex % count.x == 0) return 0;
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
            if (_stateManager.LastSelectedIndex + count.x >= _viewController.SlotUIList.Count) return 0;
            return count.x;
        }
    }
}