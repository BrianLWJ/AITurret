using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler
{
    private static Dictionary<string, ObjectPooler> poolers = new Dictionary<string, ObjectPooler>();

    private Queue<Component> poolQueue; // Queue for objects in this pool
    private Component prefab;           // Prefab for this pool
    private int maxPoolSize;            // Maximum allowed size of the pool

    // Constructor for creating a new pool
    private ObjectPooler(Component prefab, int initialPoolSize, int maxPoolSize)
    {
        this.prefab = prefab;
        this.maxPoolSize = maxPoolSize;
        poolQueue = new Queue<Component>();

        // Pre-fill the pool with inactive objects
        for (int i = 0; i < initialPoolSize; i++)
        {
            var instance = Object.Instantiate(prefab);
            instance.gameObject.SetActive(false); // Start inactive
            poolQueue.Enqueue(instance);          // Add to the pool
        }
    }

    // Setup a pool with a unique key
    public static void SetupPool<T>(T prefab, int initialPoolSize, int maxPoolSize, string key) where T : Component
    {
        if (!poolers.ContainsKey(key))
        {
            poolers[key] = new ObjectPooler(prefab, initialPoolSize, maxPoolSize);
        }
    }

    // Get an object from the pool
    public static T DequeueObject<T>(string key) where T : Component
    {
        if (!poolers.ContainsKey(key))
        {
            Debug.LogError($"Pool with key '{key}' does not exist.");
            return null;
        }

        var pool = poolers[key];

        // If the pool is not empty, retrieve an object
        if (pool.poolQueue.Count > 0)
        {
            var item = pool.poolQueue.Dequeue();
            item.gameObject.SetActive(true); // Activate the object
            return (T)item;
        }

        // If the pool is empty, instantiate a new object
        var newInstance = Object.Instantiate(pool.prefab);
        Debug.LogWarning($"Pool '{key}' is empty. Instantiating a new object.");
        return (T)newInstance;
    }

    // Return an object to the pool
    public static void EnqueueObject<T>(T item, string key) where T : Component
    {
        if (!poolers.ContainsKey(key))
        {
            Debug.LogError($"Pool with key '{key}' does not exist.");
            return;
        }

        var pool = poolers[key];

        if (item == null)
        {
            Debug.LogError("Cannot enqueue a null object.");
            return;
        }

        // Deactivate the object and reset its position (optional)
        item.gameObject.SetActive(false);
        item.transform.position = Vector3.zero;

        // If the pool is full, instead of destroying objects, add the object back to the queue
        if (pool.poolQueue.Count >= pool.maxPoolSize)
        {
            Debug.LogWarning($"Pool '{key}' is at max size. Reusing the oldest object.");
        }

        // Add the object back to the pool
        pool.poolQueue.Enqueue(item);
    }
}
