using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct BulletType
    {
        public string bulletName;
        public BulletController bulletPrefab;
        public int poolSize;
    }

    public List<BulletType> bulletTypes;

    private void Awake()
    {
        SetupBulletPools();
    }

    private void SetupBulletPools()
    {
        foreach (var bulletType in bulletTypes)
        {
            ObjectPooler.SetupPool(bulletType.bulletPrefab, bulletType.poolSize, bulletType.bulletName);
        }
    }
}
