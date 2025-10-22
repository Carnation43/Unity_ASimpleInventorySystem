using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Item[] items;

    public GameObject sourcePrefab;

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
        if (sourcePrefab != null && GameObjectPoolManager.Instance != null)
        {
            GameObjectPoolManager.Instance.ReturnToPool(sourcePrefab, this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    
    }

    public void Initialize()
    {
        randomizedItem = items[Random.Range(0, items.Length)];
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = randomizedItem.sprite;
        }
    }
}
