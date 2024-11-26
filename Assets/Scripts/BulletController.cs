using System.Collections;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 5.0f;
    public float lifetime = 3.0f;
    private Rigidbody rb;
    public Vector3 direction;
    //public string bulletName; // Unique name for the bullet's pool
    public int bulletIndex; // Unique index for the bullet's pool
    private void Awake()
    {
        // Initialize Rigidbody reference only once
        rb = GetComponent<Rigidbody>();
    }

    // Called when the bullet is fired
    public void Initialize(Vector3 fireDirection)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // Normalize the direction to ensure consistent speed
        direction = fireDirection.normalized;

        // Set the velocity in the direction of fire
        rb.linearVelocity = direction * speed;

        // Make sure the bullet is active
        gameObject.SetActive(true);

        // Reset the lifetime timer
        lifetime = 3.0f;

        // Rotate the bullet to face the direction it is moving
        if (rb.linearVelocity.sqrMagnitude > 0)
        {
            Quaternion rotation = Quaternion.LookRotation(rb.linearVelocity);
            transform.rotation = rotation;
        }

        // Start the lifetime countdown coroutine
        StartCoroutine(LifetimeCoroutine());
    }

    private IEnumerator LifetimeCoroutine()
    {
        // Wait for the lifetime of the bullet to expire
        yield return new WaitForSeconds(lifetime);

        // Return the bullet to the pool after its lifetime
        ObjectPooler.EnqueueObject(this, bulletIndex);
        //Debug.Log("BACK YOU GO (Lifetime expired)");
    }

    // If the bullet collides with something, return it to the pool
    private void OnCollisionEnter(Collision collision)
    {
        // Return the bullet to the pool on any collision
        StartCoroutine(LifetimeCoroutine());
        //ObjectPooler.EnqueueObject(this, "Bullet");
    }
}
