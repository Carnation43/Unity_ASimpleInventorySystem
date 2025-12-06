using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generic Base Class for a single Tab button
/// </summary>
/// <typeparam name="T_Category"></typeparam>
public abstract class BaseTab<T_Category> : MonoBehaviour
{
    [Header("Base Components")]
    [SerializeField] protected Image icon;
    [SerializeField] protected Color selectedColor = Color.black;
    [SerializeField] protected Color deselectedColor = Color.white;
    
    [Header("Filter Settings")]
    [SerializeField] public T_Category category;

    public virtual void OnSelect()
    {
        if (icon != null) icon.color = selectedColor;
    }

    public virtual void OnDeselect()
    {
        if (icon != null) icon.color = deselectedColor;
    }

    public abstract void OnClick();
}
