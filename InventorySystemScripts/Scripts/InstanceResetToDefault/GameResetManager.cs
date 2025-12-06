using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This file is used to uniformly reset all script files that implement the singleton pattern.
/// </summary>
namespace InstanceResetToDefault
{
    public class GameResetManager : MonoBehaviour
    {
        public static GameResetManager Instance { get; private set; }

        private List<IResettableData> runtimeDataSystems = new List<IResettableData>();

        [Header("Data Systems")]
        [SerializeField] private List<ScriptableObject> soDataSystems;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            if (soDataSystems != null)
            {
                foreach (var so in soDataSystems)
                {
                    if (so is IResettableData dataSystem)
                    {
                        Register(dataSystem);
                    }
                    else
                    {
                        Debug.LogWarning($"[GameResetManager] {so.name} 没有实现 IResettableData 接口，无法注册。");
                    }
                }
            }
        }

        // add new instance which needs to be reset
        public void Register(IResettableData resettable)
        {
            if (!runtimeDataSystems.Contains(resettable))
            {
                runtimeDataSystems.Add(resettable);
            }
        }

        public void UnRegister(IResettableData resettable)
        {
            if (runtimeDataSystems.Contains(resettable))
            {
                runtimeDataSystems.Remove(resettable);
            }
        }

        [ContextMenu("Execute New Game Reset")]
        public void ResetAllDataForNewGame()
        {
            Debug.Log("<color=red>Executing Global DATA Reset (New Game)...</color>");

            for (int i = runtimeDataSystems.Count - 1; i >= 0; i--)
            {
                var system = runtimeDataSystems[i];
                if (system != null)
                {
                    Debug.Log($" - Resetting Data: {system.GetType().Name}");
                    system.ResetData();
                }
            }
        }
    }
}

