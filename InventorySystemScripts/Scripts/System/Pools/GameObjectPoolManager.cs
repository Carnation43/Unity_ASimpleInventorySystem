using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic object pooling manager for any GameObject.
/// </summary>
public class GameObjectPoolManager : MonoBehaviour
{
    public static GameObjectPoolManager Instance { get; private set; }

    // The dictionary now stores a Queue of GameObjects.
    private Dictionary<GameObject, Queue<GameObject>> _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance);

        Debug.Log("GameObjectPoolManager has AWAKENED!");
    }

    /// <summary>
    /// Retrieves a GameObject instance from the pool
    /// </summary>
    public GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!_poolDictionary.ContainsKey(prefab))
        {
            _poolDictionary.Add(prefab, new Queue<GameObject>());
        }

        GameObject objectToSpawn;

        if (_poolDictionary[prefab].Count > 0)
        {
            objectToSpawn = _poolDictionary[prefab].Dequeue();

            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.gameObject.SetActive(true);
        }
        else
        {
            // No objects in the pool, instantiate a new one
            objectToSpawn = Instantiate(prefab, position, rotation);
        }

        return objectToSpawn;
    }

    /// <summary>
    /// Returns a GameObject instance to the pool
    /// </summary>
    public void ReturnToPool(GameObject prefab, GameObject objectToReturn)
    {
        if (!_poolDictionary.ContainsKey(prefab))
        {
            _poolDictionary.Add(prefab, new Queue<GameObject>());
        }

        objectToReturn.gameObject.SetActive(false);
        _poolDictionary[prefab].Enqueue(objectToReturn);
    }
}
