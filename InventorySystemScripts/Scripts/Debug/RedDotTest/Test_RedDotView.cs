using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Test_RedDotView : MonoBehaviour
{
    [SerializeField] private GameObject redDotGo;
    [SerializeField] private TMP_Text redDotCount;

    private Test_RedDotSystem.Test_RedDotNode _targetNode;

    private string _currentPath;

    public void SetPath(string path)
    {
        Unsubscribe();
        _currentPath = path;
        if (string.IsNullOrEmpty(path)) return;

        _targetNode = Test_RedDotSystem.Instance.GetNode(path, true);

        if (_targetNode != null)
        {
            UpdateVisuals(_targetNode.Count);
            Subscribe();
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (_targetNode != null)
        {
            _targetNode.OnCountChanged += UpdateVisuals;
        }
    }

    private void Unsubscribe()
    {
        if (_targetNode != null)
        {
            _targetNode.OnCountChanged -= UpdateVisuals;
        }
    }

    private void UpdateVisuals(int count)
    {
        if (count > 0)
        {
            redDotGo.SetActive(true);
            redDotCount.text = _targetNode.Count.ToString();
        }
        else
        {
            redDotGo.SetActive(false);
        }
    }
}
