using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] Image icon;
    [SerializeField] TMP_Text stackCountText;

    private void Awake()
    {
        
    }

    public void Initialize(InventorySlot newItem)
    {
        icon.gameObject.GetComponent<Image>().enabled = (newItem != null);

        if(newItem == null)
        {
            stackCountText.gameObject.SetActive(false);
            return;
        }

        icon.sprite = newItem.item.sprite;

        stackCountText.text = newItem.count.ToString();
        stackCountText.gameObject.SetActive(newItem.count > 1);
    }

}
