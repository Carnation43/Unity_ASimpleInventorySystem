using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryNavigationHandler : MonoBehaviour
{
    private MenuController _menuController;

    public void Initialize(MenuController menuController)
    {
        _menuController = menuController;
    }

    public void HandleNavigationInput()
    {
        if (_menuController.LastItemSelected == null) return;

        Vector2 move = UserInput.UIMoveInput;

        if (move.x > 0)
        {
            int add = CalculateXAddition(1);
            HandleNextItemSelection(add);
        }
        else if (move.x < 0)
        {
            int add = CalculateXAddition(-1);
            HandleNextItemSelection(add);
        }
        else if (move.y > 0)
        {
            int add = CalculateYAddition(1);
            HandleNextItemSelection(add);
        }
        else if (move.y < 0)
        {
            int add = CalculateYAddition(-1);
            Debug.Log("MoveInput Y < 0: add = " + add);
            HandleNextItemSelection(add);
        }
    }

    private void HandleNextItemSelection(int addition)
    {
        int newIndex = _menuController.LastSelectedIndex + addition;
        if (newIndex < 0)
        {
            EventSystem.current.SetSelectedGameObject(_menuController.LastItemSelected);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(_menuController.inventorySlots[newIndex].gameObject);
        }
    }

    private int CalculateXAddition(int direction)
    {
        Vector2Int count = GridLayoutGroupHelper.Size(_menuController._group);
        if (direction > 0)
        {
            // if last slot or not
            if (_menuController.LastSelectedIndex == _menuController.inventorySlots.Count - 1)
            {
                return 0;
            }
            if (_menuController.LastSelectedIndex % count.x == count.x - 1)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        if (direction < 0)
        {
            if (_menuController.LastSelectedIndex % count.x == 0)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        return 0;
    }

    private int CalculateYAddition(int direction)
    {
        if (direction > 0)
        {
            Vector2Int count = GridLayoutGroupHelper.Size(_menuController._group);
            if (_menuController.LastSelectedIndex - count.x < 0)
            {
                return 0;
            }
            else
            {
                return -count.x;
            }
        }
        else if (direction < 0)
        {
            Debug.Log("direction = " + direction);
            Vector2Int count = GridLayoutGroupHelper.Size(_menuController._group);
            // Assure that the last line cannot move downward.
            if (_menuController.LastSelectedIndex + count.x >= _menuController.inventorySlots.Count)
            {
                Debug.Log("当前数组大小：" + _menuController.inventorySlots.Count);
                Debug.Log("要移动的行数：" + count.x);
                Debug.Log("当前选中项index：" + _menuController.LastSelectedIndex);
                return 0;
            }
            else
            {
                return count.x;
            }
        }
        return 0;
    }
}