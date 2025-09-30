using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This file is used to uniformly reset all script files that implement the singleton pattern.
/// </summary>
namespace InstanceResetToDefault
{
    public class SingletonResetManager : MonoBehaviour
    {
        public static SingletonResetManager Instance { get; private set; }

        private List<IResettable> resettableSingletons = new List<IResettable>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject);
            }
        }

        // add new instance which needs to be reset
        public void Register(IResettable resettable)
        {
            if (!resettableSingletons.Contains(resettable))
            {
                resettableSingletons.Add(resettable);
            }
        }

        public void UnRegister(IResettable resettable)
        {
            if (resettableSingletons.Contains(resettable))
            {
                resettableSingletons.Remove(resettable);
            }
        }

        public void ResetAllSingletons()
        {
            foreach (var resettable in resettableSingletons.ToArray())
            {
                Debug.Log($"- Resetting: {resettable.GetType().Name}");
                resettable.ResetToDefaultState();
            }
        }
    }
}

