using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    private float lifeTimer;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            ObjectPooler.EnqueueObject("Bullet", this.gameObject);
        }
    }

    public void Initialize(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        lifeTimer = 0;
    }
}
