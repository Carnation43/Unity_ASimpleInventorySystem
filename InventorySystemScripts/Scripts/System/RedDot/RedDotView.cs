using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A reusable component for Red Dot System
/// </summary>
public class RedDotView : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string staticPath;

    [Header("UI References")]
    [SerializeField] private GameObject redDotGo;

    private RedDotManager.RedDotNode _targetNode;
    private string _currentPath;

    private void Start()
    {
        if (_targetNode == null)
            UpdateVisuals(0);
    }

    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(_currentPath))
        {
            SetPath(_currentPath);
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    /// <summary>
    /// Sets the Red Dot dynamically.
    /// </summary>
    public void SetPath(string path)
    {
        Unsubscribe();

        _currentPath = path;

        if (RedDotManager.Instance == null) return;

        _targetNode = RedDotManager.Instance.GetRedDotNode(path);

        if (_targetNode != null)
        {
            // Subscribe to event
            _targetNode.OnCountChanged += UpdateVisuals;

            UpdateVisuals(_targetNode.Count);
        }
    }

    private void UpdateVisuals(int count)
    {
        if (redDotGo != null)
        {
            if (count > 0)
                redDotGo.SetActive(true);
            else
                redDotGo.SetActive(false);
        }
    }

    private void Unsubscribe()
    {
        if (_targetNode != null)
        {
            _targetNode.OnCountChanged -= UpdateVisuals;
            _targetNode = null;
        }
    }
}
