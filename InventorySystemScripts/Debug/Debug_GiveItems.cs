using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_GiveItems : MonoBehaviour
{
    [System.Serializable]
    public class ItemToGive
    {
        public Item item;
        [Range(1, 99)]
        public int quantity = 1;
    }

    [SerializeField] private List<ItemToGive> itemsToGiveOnStart;

    private void Start()
    {
        foreach (var itemEntry in itemsToGiveOnStart)
        {
            if (itemEntry.item != null)
            {
                // ���������õ���������ε��� AddItem ����
                for (int i = 0; i < itemEntry.quantity; i++)
                {
                    InventoryManager.instance.AddItem(itemEntry.item);
                }
                Debug.Log($"[DEBUG] ����� {itemEntry.quantity} �� {itemEntry.item.name}��");
            }
        }
    }
}
