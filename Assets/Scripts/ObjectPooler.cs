using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler
{
    // Dictionary to store multiple pools, identified by unique keys
    private static Dictionary<string, ObjectPooler> poolers = new Dictionary<string, ObjectPooler>();
    private Queue<Component> poolQueue;
    private Component prefab;

    // Private constructor to prevent external instantiation
    private ObjectPooler(Component prefab, int initialPoolSize)
    {
        poolQueue = new Queue<Component>();
        this.prefab = prefab;

        // Instantiate the initial pool of objects
        for (int i = 0; i < initialPoolSize; i++)
        {
            var instance = Object.Instantiate(prefab);
            instance.gameObject.SetActive(false);
            poolQueue.Enqueue(instance);
        }
    }

    // Method to get or create a pool for the specified key and prefab
    public static void SetupPool<T>(T prefab, int initialPoolSize, string key) where T : Component
    {
        if (!poolers.ContainsKey(key))
        {
            poolers[key] = new ObjectPooler(prefab, initialPoolSize);
        }
    }

    // Enqueue an object back into the pool
    public static void EnqueueObject<T>(T item, string key) where T : Component
    {
        if (poolers.ContainsKey(key) && item.gameObject.activeSelf)
        {
            item.transform.position = Vector3.zero;
            item.gameObject.SetActive(false);
            poolers[key].poolQueue.Enqueue(item);
        }
    }

    // Dequeue an object from the pool
    public static T DequeueObject<T>(string key) where T : Component
    {
        if (poolers.ContainsKey(key) && poolers[key].poolQueue.Count > 0)
        {
            var item = poolers[key].poolQueue.Dequeue();
            item.gameObject.SetActive(true);
            return (T)item;
        }
        else if (poolers.ContainsKey(key))
        {
            // If no available item in pool, instantiate a new one
            var newInstance = Object.Instantiate(poolers[key].prefab);
            return (T)newInstance;
        }

        Debug.LogError($"Pool with key '{key}' does not exist.");
        return null;
    }
}
