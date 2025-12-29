using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IGridView
{
    IReadOnlyList<Selectable> SelectableSlots { get; }

    /// <summary>
    /// Get item count.
    /// </summary>
    int TotalDataCount { get; }

    /// <summary>
    /// Get the column count of the grid.
    /// </summary>
    int ColumnCount { get; }

    /// <summary>
    /// Get the index of the current game object.
    /// </summary>
    /// <param name="slotObj">UI Slot</param>
    /// <returns>from (0) to (TotalDataCount - 1)</returns>
    int GetDataIndex(GameObject slotObj);

    /// <summary>
    /// Request a certain data index.
    /// </summary>
    void SelectDataIndex(int dataIndex);
}
