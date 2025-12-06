using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic interface that defines the functions that all "grid slot UIs" must implement.
/// </summary>
/// <typeparam name="T_SlotData"></typeparam>
public interface ISlotUI<T_SlotData>
{
    T_SlotData IData { get; }

    void Initialize(T_SlotData data);
}
