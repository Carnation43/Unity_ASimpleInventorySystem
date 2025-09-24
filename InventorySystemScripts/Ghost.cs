using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    [SerializeField] GameObject itemPrefab;

    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnMouseDown()
    {
        animator.SetTrigger("Hit");
        DropItem();
    }

    void DropItem()
    {
        GameObject newGo = Instantiate(itemPrefab, transform.position, transform.rotation);
        Rigidbody2D rb = newGo.GetComponent<Rigidbody2D>();

        rb.AddForce(new Vector2(Random.Range(-5f, 5f), 4f), ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-10f, 10f));
    }
}
