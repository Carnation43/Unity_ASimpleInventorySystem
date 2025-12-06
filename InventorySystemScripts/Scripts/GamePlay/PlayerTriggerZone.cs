using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A generic trigger zone for scene actions
/// </summary>
public class PlayerTriggerZone : MonoBehaviour
{
    [Header("Trigger Events")]
    public UnityEvent onPlayerEnter;

    public UnityEvent onPlayerExit;

    [Header("Settings")]
    [Tooltip("whether to trigger once or not")]
    [SerializeField] private bool oneShot = false;

    private bool _hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (oneShot && _hasTriggered) return;

        if (collision.CompareTag("Player"))
        {
            onPlayerEnter?.Invoke();

            if (oneShot) _hasTriggered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (oneShot && _hasTriggered) return;

        if (collision.CompareTag("Player"))
        {
            onPlayerExit?.Invoke();
        }
    }
}
