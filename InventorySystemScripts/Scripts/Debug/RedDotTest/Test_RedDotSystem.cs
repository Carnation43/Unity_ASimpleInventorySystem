using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_RedDotSystem : MonoBehaviour
{
    public static Test_RedDotSystem Instance { get; private set; }

    private Test_RedDotNode _rootNode;

    private const char PATH_SEPARATOR = '/';

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        InitializeTree();
    }

    private void InitializeTree()
    {
        _rootNode = new Test_RedDotNode("Root");
    }

    public Test_RedDotNode GetNode(string path, bool createIfMissing)
    {
        if (string.IsNullOrEmpty(path)) return null;

        string[] pathArray = path.Split(PATH_SEPARATOR);

        if (pathArray[0] != "Root" || pathArray.Length <= 0) return null;

        Test_RedDotNode _currentNode = _rootNode;

        for (int i = 1; i < pathArray.Length; i++)
        {
            string pathName = pathArray[i];
            Test_RedDotNode childDot = _currentNode.GetChild(pathName);

            if (childDot == null)
            {
                if (!createIfMissing) return null;

                childDot = new Test_RedDotNode(pathName);
                _currentNode.AddChild(childDot);
            }

            _currentNode = childDot;
        }

        return _currentNode;
    }

    public class Test_RedDotNode
    {
        public event Action<int> OnCountChanged;

        public string Name { get; private set; }
        public int Count { get; private set; }
        public Test_RedDotNode Parent { get; private set; }

        public Dictionary<string, Test_RedDotNode> Children { get; private set; }

        public Test_RedDotNode(string name)
        {
            Name = name;
            Count = 0;
            Children = new Dictionary<string, Test_RedDotNode>();
        }

        public Test_RedDotNode GetChild(string name)
        {
            if (Children.TryGetValue(name, out Test_RedDotNode node))
            {
                return node;
            }
            return null;
        }

        public void AddChild(Test_RedDotNode child)
        {
            if (!Children.ContainsKey(child.Name))
            {
                Children.Add(child.Name, child);
                child.Parent = this;
            }
        }

        public void SetCount(int newCount)
        {
            if (Count == newCount) return;

            int diff = newCount - Count;

            Count = newCount;

            OnCountChanged?.Invoke(Count);

            Parent?.ApplyChange(diff);
        }

        public void ApplyChange(int diff)
        {
            if (diff == 0) return;

            Count += diff;

            OnCountChanged?.Invoke(Count);

            Parent?.ApplyChange(diff);
        }
    }

    public static class Test_RedDotPaths
    {
        // Root
        // -- MainMail
        // ---- SubMail
        // -------- Mail(1)
        public const string Root = "Root";

        public const string MainMail = Root + "/MainMail";

        public const string SubMail = MainMail + "/SubMail";
        public const string SubSpecialMail = MainMail + "/SubSpecialMail";

        public const string Mail = SubMail + "/Mail";
        public const string SpecialMail = SubSpecialMail + "/SpecialMail";
    }
}
