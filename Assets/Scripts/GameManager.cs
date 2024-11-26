using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct BulletType
    {
        public int bulletIndex;            // The name to identify the bullet type
        public BulletController bulletPrefab; // The bullet prefab for the pool
        public int poolSize;                 // The initial pool size
        public int maxPoolSize;              // The maximum pool size
    }

    public List<BulletType> bulletTypes;     // List of bullet types for different pools

    private void Awake()
    {
        SetupBulletPools();
    }

    private void SetupBulletPools()
    {
        // Iterate through all bullet types and set up their respective pools
        foreach (var bulletType in bulletTypes)
        {
            ObjectPooler.SetupPool(
                bulletType.bulletPrefab,
                bulletType.poolSize,
                bulletType.maxPoolSize,
                bulletType.bulletIndex
            );
        }
    }
}
