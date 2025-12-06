using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_SimpleInteractPrompt : MonoBehaviour
{
    [SerializeField] private GameObject promptGo;

    private void Awake()
    {
        promptGo.SetActive(false);
    }

    public void ShowPrompt() => promptGo.SetActive(true);
    public void HidePrompt() => promptGo.SetActive(false);
}
