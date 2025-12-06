using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the state and data of the radial menu.
/// </summary>
public class RadialMenuModel : MonoBehaviour
{
    [Header("Broadcasting On")]
    [SerializeField] private InputEventChannel inputChannel;

    // Events

    /// <summary>
    /// Invoked when the menu's open state changes.
    /// </summary>
    public event Action<bool> OnMenuStateChanged;

    /// <summary>
    /// Invoked when the highlighted item index change.
    /// The integer parameter is the new index of the highlighted item.
    /// </summary>
    public event Action<int> OnHighlightIndexChanged;

    /// <summary>
    /// Invoked when a selection is confirmed.
    /// The RadialMenuData parameter is the data of the selected item.
    /// </summary>
    public event Action<RadialMenuData> OnItemSelected;

    // Stats
    public bool IsOpen { get; private set; }
    private int _currentHighlightIndex = -1;

    /// <summary>
    /// Gets or sets the index of the currently highlighted item.
    /// Setting a new value will invoke the OnHighlightIndexChanged event if the index has changed.
    /// An index of -1 means no item is currently highlighted.
    /// </summary>
    public int CurrentHighlightIndex
    {
        get => _currentHighlightIndex;
        set
        {
            if (_currentHighlightIndex != value)
            {
                _currentHighlightIndex = value;
                OnHighlightIndexChanged?.Invoke(_currentHighlightIndex);
            }
        }
    }

    public List<RadialMenuData> currentItems { get; private set; }

    /// <summary>
    /// Opens the radial menu with a given list of items.
    /// </summary>
    /// <param name="items">The list of RadialMenuData items to display.</param>
    public void OpenMenu(List<RadialMenuData> items)
    {
        if (IsOpen) return;

        currentItems = items;
        IsOpen = true;
        OnMenuStateChanged?.Invoke(true);

        inputChannel?.RaiseGlobalInputLockEvent(true);
    }

    public void CloseMenu()
    {
        if (!IsOpen) return;

        IsOpen = false;
        CurrentHighlightIndex = -1;
        OnMenuStateChanged?.Invoke(false);

        inputChannel?.RaiseGlobalInputLockEvent(false);
    }

    public void ConfirmSelection()
    {
        if (!IsOpen || CurrentHighlightIndex < 0 || CurrentHighlightIndex >= currentItems.Count)
        {
            return;
        }

        RadialMenuData selectedData = currentItems[CurrentHighlightIndex];
        Debug.Log("Clicked the radial part...");

        selectedData.OnConfirmAction?.Invoke();

        OnItemSelected?.Invoke(selectedData);
    }

}
