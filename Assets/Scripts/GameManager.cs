using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BulletController bulletPrefab;
    private void Awake()
    {
        SetupPool();
    }

    private void SetupPool()
    {
        ObjectPooler.SetupPool(bulletPrefab, 10, "Bullet");
        //ObjectPooler.Instance.SetupPool("Bullet", bulletPrefab, 10);
    }
}
