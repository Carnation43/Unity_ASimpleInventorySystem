using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IGridView
{
    IReadOnlyList<Selectable> SelectableSlots { get; }
}
