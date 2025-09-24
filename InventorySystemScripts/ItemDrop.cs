using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Item[] items;

    // Temp
    SpriteRenderer spriteRenderer;
    Item randomizedItem;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        randomizedItem = items[Random.Range(0, items.Length)];
        spriteRenderer.sprite = randomizedItem.sprite;
    }

    private void OnMouseDown()
    {
        InventoryManager.instance.AddItem(randomizedItem);
        Destroy(gameObject);
    }
}
