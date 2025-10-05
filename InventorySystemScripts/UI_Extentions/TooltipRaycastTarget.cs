using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipRaycastTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsPointerOver { get; private set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOver = false;
    }

    private void OnDisable()
    {
        IsPointerOver = false;
    }
}
