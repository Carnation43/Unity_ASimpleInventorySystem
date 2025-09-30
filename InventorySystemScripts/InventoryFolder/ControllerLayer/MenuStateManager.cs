using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuStateManager : MonoBehaviour
{
    public GameObject LastItemSelected { get; set; }
    public int LastSelectedIndex { get; set; }

    // This public method is used to respond to the "item selected" event.
    public void OnItemSelected(GameObject selectedObject, int index)
    {
        LastItemSelected = selectedObject;
        LastSelectedIndex = index;
    }

    // This method is used to clear the state when opening a menu or when a reset is needed.
    public void ClearSelection()
    {
        LastItemSelected = null;
        LastSelectedIndex = 0;
    }
}
