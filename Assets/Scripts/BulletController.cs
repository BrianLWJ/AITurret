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
        //Initialize Rigidbody reference
        rb = GetComponent<Rigidbody>();
    }

    //Called when  bullet is fired
    public void Initialize(Vector3 fireDirection)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        //Normalize direction, ensuring consistent speed
        direction = fireDirection.normalized;

        //Set velocity in direction of fire
        rb.linearVelocity = direction * speed;

        //Bullet active
        gameObject.SetActive(true);

        // Reset the lifetime timer
        lifetime = 3.0f;

        //Rotate bullet to face the direction it's moving
        if (rb.linearVelocity.sqrMagnitude > 0)
        {
            Quaternion rotation = Quaternion.LookRotation(rb.linearVelocity);
            transform.rotation = rotation;
        }

        // Start the lifetime countdown
        StartCoroutine(LifetimeCoroutine());
    }

    private IEnumerator LifetimeCoroutine()     //Bullet LifeTimer
    {
        //Wait for lifetime of bullet
        yield return new WaitForSeconds(lifetime);

        //Return bullet to pool after lifetime
        ObjectPooler.EnqueueObject(this, bulletIndex);
    }

    private void OnCollisionEnter(Collision collision)      // Return bullet to pool on collision

    {        
        StartCoroutine(LifetimeCoroutine());    // Start the lifetime countdown.    Possibly No need this, IT Depends
        //ObjectPooler.EnqueueObject(this, "Bullet");
        //ObjectPooler.EnqueueObject(this, bulletIndex);

    }
}
