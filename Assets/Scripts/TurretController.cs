using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Turret
{
    public class TurretController : MonoBehaviour
    {
        public Transform player;
        public float shootingRange = 10f;
        public float fireRate = 1f; // Fire rate in seconds
        public float reloadTime = 6f;
        public int maxAmmo = 30;
        private int currentAmmo;
        private float fireCooldown;
        public float overHeatTime = 10f;
        public float fadeDuration = 1f;

        public Transform turretBody;
        private Animator animator;
        private StateMachine stateMachine;

        private int readyOverHeat = 0;
        public bool overheatBool = false;

        public Color overheatColor = Color.red;
        public Color idleColor = Color.green;
        public Color shootColor = Color.blue;
        public Color reloadColor = Color.yellow;

        public Transform firePoint;
        public string bulletType = "StandardBullet"; // Specify the bullet type by name here
        private Renderer turretRenderer;
        
        private void Awake()
        {
            currentAmmo = maxAmmo;
            fireCooldown = 0f;
            turretRenderer = GetComponent<Renderer>();
            //originalColor = turretRenderer.material.color;
        }

        private void Start()
        {
            SetupStateMachine();
        }

        private void SetupStateMachine()
        {
            stateMachine = new StateMachine();

            var idleState = new IdleState(this, animator);
            var shootingState = new ShootingState(this, animator);
            var reloadingState = new ReloadingState(this, animator);
            var overheatState = new OverheatState(this, animator);

            At(idleState, shootingState, new FuncPredicate(() => IsTargetInRange(shootingRange) && HasLineOfSight()));
            At(shootingState, overheatState, new FuncPredicate(() => overheatBool));
            At(overheatState, shootingState, new FuncPredicate(() => IsTargetInRange(shootingRange) && !overheatBool));

            At(shootingState, reloadingState, new FuncPredicate(() => IsMaxAmmo()));
            At(reloadingState, shootingState, new FuncPredicate(() => IsTargetInRange(shootingRange) && HasLineOfSight()));

            // Back to idle state
            At(shootingState, idleState, new FuncPredicate(() => !IsTargetInRange(shootingRange) && !HasLineOfSight()));
            At(overheatState, idleState, new FuncPredicate(() => !IsTargetInRange(shootingRange) && !HasLineOfSight()));
            At(reloadingState, idleState, new FuncPredicate(() => !IsTargetInRange(shootingRange) && !HasLineOfSight()));

            Any(idleState, new FuncPredicate(ReturnToIdleState)); //IDK HOW DOES THIS LINE WORKS, PROBABLY THE 3 LINES ABOVE ALREADY USE THIS LINE AS BASE. IDK

            stateMachine.SetState(idleState);
        }

        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        private void Update()
        {
            stateMachine.Update();
            fireCooldown -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        // Idle state logic
        public void OnIdle()
        {
            turretRenderer.material.color = idleColor; // Change to green when idle

            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
            turretBody.rotation = Quaternion.Slerp(turretBody.rotation, targetRotation, Time.deltaTime * 25f); // Adjust the speed as needed
        }

        // Tracking logic
        private bool IsTargetInRange(float range)
        {
            if (player == null) return false;
            return Vector3.Distance(transform.position, player.position) <= range;
        }

        public void TrackPlayer()
        {
            Vector3 direction = player.position - turretBody.position;
            direction.y = 0; // Keep the rotation on the horizontal plane
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            turretBody.rotation = Quaternion.Slerp(turretBody.rotation, targetRotation, Time.deltaTime * 300f);
        }

        private bool HasLineOfSight()
        {
            if (player == null) return false;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
            return false;
        }

        // Shooting logic
        public void Shoot()
        {
            if (currentAmmo > 0 && fireCooldown <= 0f && !overheatBool)
            {
                turretRenderer.material.color = shootColor;
                FireBullet(bulletType);

                currentAmmo--;
                fireCooldown = fireRate; // Reset fire cooldown
                readyOverHeat++;

                if (readyOverHeat >= 15) // Trigger overheating after 15 shots
                {
                    overheatBool = true; // Set the overheating flag
                    StartOverheating();
                }
            }
            else if (currentAmmo <= 0)
            {
                Reload();
            }
        }

        public void FireBullet(string bulletName)
        {
            var bullet = ObjectPooler.DequeueObject<BulletController>(bulletName);
            if (bullet != null)
            {
                // Position bullet at the fire point and initialize it
                bullet.transform.position = firePoint.position;
                bullet.gameObject.SetActive(true);

                Vector3 directionToPlayer = (player.position - firePoint.position).normalized;
                bullet.Initialize(directionToPlayer);
            }
        }

        // Overheat logic
        public void StartOverheating()
        {
            turretRenderer.material.color = overheatColor; // Change to red when overheating
            StartCoroutine(OverHeatCoroutine());
        }

        private IEnumerator OverHeatCoroutine()
        {
            yield return new WaitForSeconds(overHeatTime);
             // Return to original color after cooling down
            if (!IsTargetInRange(shootingRange) && !HasLineOfSight()){
                turretRenderer.material.DOColor(idleColor, fadeDuration);
            }
            else
            {
                turretRenderer.material.DOColor(shootColor, fadeDuration);
            }
            overheatBool = false;
            readyOverHeat = 0; // Reset overheating counter
        }

        private bool IsOverheated()
        {
            return overheatBool; // Check if the turret is overheated
        }

        // Reloading logic
        private bool IsMaxAmmo()
        {
            return currentAmmo >= maxAmmo;
        }

        public void Reload()
        {
            StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            turretRenderer.material.color = reloadColor; // Change to yellow when reloading
            yield return new WaitForSeconds(reloadTime);
            currentAmmo = maxAmmo;

            if (IsTargetInRange(shootingRange) && HasLineOfSight())
            {
                // If the player is still in range, change to blue and continue shooting
                turretRenderer.material.DOColor(shootColor, fadeDuration);
            }
            else
            {
                // If the player is out of range, change to green and return to idle
                turretRenderer.material.DOColor(idleColor, fadeDuration);
            }
        }

        public bool NeedsReloading()
        {
            return currentAmmo <= 0;
        }

        // Back to idle
        private bool ReturnToIdleState()
        {
            return false; // Update this condition as needed
        }
    }
}
