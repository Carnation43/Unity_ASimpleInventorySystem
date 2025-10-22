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
                // 根据你设置的数量，多次调用 AddItem 方法
                for (int i = 0; i < itemEntry.quantity; i++)
                {
                    InventoryManager.instance.AddItem(itemEntry.item);
                }
                Debug.Log($"[DEBUG] 添加了 {itemEntry.quantity} 个 {itemEntry.item.name}。");
            }
        }
    }
}
