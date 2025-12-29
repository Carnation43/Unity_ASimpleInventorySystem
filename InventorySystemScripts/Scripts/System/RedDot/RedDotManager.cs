using InstanceResetToDefault;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// The core manager for the Red Dot System
/// Implements a tree-based structure to manage notification counts.
/// </summary>
public class RedDotManager : MonoBehaviour, IResettableData
{
    public static RedDotManager Instance { get; private set; }

    /// <summary>
    /// Path: "Root"
    /// </summary>
    private RedDotNode _redDotNode;

    // Separator used to parse paths
    private const char PATH_SEPARATOR = '/';

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeTree();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeTree()
    {
        _redDotNode = new RedDotNode("Root");
    }

    private void OnEnable()
    {
        if (GameResetManager.Instance != null)
        {
            GameResetManager.Instance.Register(this);
        }
    }

    private void OnDisable()
    {
        if (GameResetManager.Instance != null)
        {
            GameResetManager.Instance.UnRegister(this);
        }
    }

    public void ResetData()
    {
        Debug.Log("[RedDotManager] Resetting data...");
        InitializeTree();
    }

    #region public API
    /// <summary>
    /// Registers a listener for a specific path.
    /// </summary>
    /// <param name="path">The full path to listen to</param>
    /// <param name="callback">The UI function to call when data changes</param>
    public void RegisterCallback(string path, Action<int> callback)
    {
        RedDotNode node = GetNode(path, createIfMissing: true);

        if (node != null)
        {
            node.OnCountChanged += callback;

            // notify the UI of the current value immediately
            callback?.Invoke(node.Count);
        }
    }

    public void UnregisterCallback(string path, Action<int> callback)
    {
        RedDotNode node = GetNode(path, false);

        if (node != null)
        {
            node.OnCountChanged -= callback;
        }
    }

    public RedDotNode GetRedDotNode(string path)
    {
        return GetNode(path, createIfMissing: true);
    }
    #endregion

    #region internal logic
    private RedDotNode GetNode(string path, bool createIfMissing)
    {
        // security check
        if (string.IsNullOrEmpty(path)) return null;

        // if path = "Root/SideMenu/Recipe" 
        // return {[0]:"Root",[1]:"SideMenu",[2]:"Recipe"}
        string[] splitPath = path.Split(PATH_SEPARATOR);

        if (splitPath.Length == 0 || splitPath[0] != "Root")
        {
            Debug.LogWarning($"[RedDotManager]: Invalid path: {path}. Root missing.");
            return null;
        }

        RedDotNode currentNode = _redDotNode;

        for (int i = 1; i < splitPath.Length; i++)
        {
            string childName = splitPath[i];

            RedDotNode childNode = currentNode.GetChild(childName);

            if (childNode == null)
            {
                // not allowed to create
                if (!createIfMissing) return null;

                // lazy load
                childNode = new RedDotNode(childName);
                currentNode.AddChild(childNode);
            }

            currentNode = childNode;
        }

        return currentNode;
    }
    #endregion

    /// <summary>
    /// Internal class representing a node in the Red Dot Tree
    /// </summary>
    public class RedDotNode
    {
        // dot name
        public string Name { get; private set; }
        // internal count
        public int Count { get; private set; }
        // dot parent
        public RedDotNode Parent { get; private set; }

        // Event triggered when this node's value changes
        public event Action<int> OnCountChanged;

        // Key: Child Name | Value: Child Node
        public Dictionary<string, RedDotNode> Children { get; private set; }

        public RedDotNode (string name)
        {
            Name = name;
            Count = 0;
            Children = new Dictionary<string, RedDotNode>();
        }

        // Query child nodes owned by current node
        public RedDotNode GetChild(string name)
        {
            if (Children.TryGetValue(name, out RedDotNode node))
            {
                return node;
            }
            return null;
        }

        public void AddChild(RedDotNode child)
        {
            if (!Children.ContainsKey(child.Name))
            {
                Children.Add(child.Name, child);
                child.Parent = this;
            }
        }

        /// <summary>
        /// Sets the count for leaf node.
        /// Changes will be handled in the parent node
        /// </summary>
        public void SetCount(int newCount)
        {
            if (Count == newCount) return;

            int diff = newCount - Count;

            Count = newCount;

            OnCountChanged?.Invoke(Count);

            Parent?.ApplyChange(diff);
        }

        /// <summary>
        /// Applies a delta change from a child node
        /// </summary>
        public void ApplyChange(int delta)
        {
            if (delta == 0) return;

            Count += delta;

            OnCountChanged?.Invoke(Count);

            Parent?.ApplyChange(delta);
        }
    }

    #region Debug Tools
    [ContextMenu("DEBUG: Print Red Dot Tree")]
    public void Debug_PrintRedTree()
    {
        if (_redDotNode == null)
        {
            Debug.LogWarning("[RedDotManager] Tree not initialized.");
            return;
        }

        // a note book
        StringBuilder sb = new StringBuilder();

        // first line
        sb.AppendLine("<color=yellow>--- Red Dot Tree Structure ---</color>");

        // Recursive call
        PrintNodeRecursive(_redDotNode, "", sb);

        Debug.Log(sb.ToString());
    }

    private void PrintNodeRecursive(RedDotNode redDotNode, string indient, StringBuilder sb)
    {
        string valueStr = redDotNode.Count > 0 ? $"<color=red>{redDotNode.Count}</color>" : redDotNode.Count.ToString();

        sb.AppendLine($"{indient}[{redDotNode.Name}]: {valueStr}");

        foreach (var child in redDotNode.Children.Values)
        {
            PrintNodeRecursive(child, indient + "       ", sb);
        }
    }
    #endregion
}
