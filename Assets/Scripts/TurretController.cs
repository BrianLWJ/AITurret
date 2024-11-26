using UnityEngine;
using System.Collections;
using DG.Tweening; //Fade Color Smoothly

namespace Turret
{
    public class TurretController : MonoBehaviour
    {
        [Header("Turret Properties")]
        public Transform turretBody;
        private Renderer turretRenderer;
        private Animator animator;
        private StateMachine stateMachine;
        public Transform firePoint;

        [Header("Player Detector")]
        public Transform player;
        public float shootingRange = 10f;

        [Header("Shooting Logic")]
        public float fireRate = 1f;
        public float reloadTime = 6f;
        public int maxAmmo = 30;
        private int currentAmmo;
        public float overHeatTime = 10f;
        private float fireCooldown;

        [Header("Overheat")]
        public int readyOverHeat = 0;
        public bool overheatBool = false;

        [Header("Turret States' Color")]
        public Color overheatColor = Color.red;
        public Color idleColor = Color.green;
        public Color shootColor = Color.blue;
        public Color reloadColor = Color.yellow;
        public float fadeDuration = 1f;

        [Header("Object Pooler")]
        //public string bulletType = "StandardBullet"; //bullet name
        public int bulletIndex; //Number of Bullet Index


        private void Awake() //call before Start()
        {
            currentAmmo = maxAmmo;  //Turret Magazine is Full upon Launch
            fireCooldown = 0f;      //Ensure fire cooldown = 0 so bullet shooot consistent
            turretRenderer = GetComponent<Renderer>(); //To get renderer, enable turret change color
        }

        private void Start()
        {
            SetupStateMachine(); //Setup State Machine Upon Launch
        }

        private void SetupStateMachine()
        {   //Initialization for StateMachine
            stateMachine = new StateMachine();

            //Declare States for Turrets
            var idleState = new IdleState(this, animator);
            var shootingState = new ShootingState(this, animator);
            var reloadingState = new ReloadingState(this, animator);
            var overheatState = new OverheatState(this, animator);

            //Logic of Transition between Turret States
            At(idleState, shootingState, new FuncPredicate(() => IsTargetInRange(shootingRange) && HasLineOfSight()));
            At(shootingState, overheatState, new FuncPredicate(() => overheatBool));
            At(overheatState, shootingState, new FuncPredicate(() => IsTargetInRange(shootingRange) && !overheatBool));

            At(shootingState, reloadingState, new FuncPredicate(() => IsMaxAmmo()));
            At(reloadingState, shootingState, new FuncPredicate(() => IsTargetInRange(shootingRange) && HasLineOfSight()));

            // Back to Idle state
            At(shootingState, idleState, new FuncPredicate(() => !IsTargetInRange(shootingRange) && !HasLineOfSight()));
            At(overheatState, idleState, new FuncPredicate(() => !IsTargetInRange(shootingRange) && !HasLineOfSight() && !overheatBool));
            At(reloadingState, idleState, new FuncPredicate(() => !IsTargetInRange(shootingRange) || !HasLineOfSight() && !IsMaxAmmo()));

            //Any(idleState, new FuncPredicate(ReturnToIdleState)); //Should have Use this for Back to Idle State
            //Any(idleState, new FuncPredicate(() => !IsTargetInRange(shootingRange) && !HasLineOfSight()));

            //Set Intial State
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
                FireBullet(bulletIndex);

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

        public void FireBullet(int bulletName)
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
            if (!IsTargetInRange(shootingRange) && !HasLineOfSight())
            {
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
        public void ChangeTurretColor(Color targetColor)
        {
            //if (turretRenderer != null)
            //{
            //    turretRenderer.material.color = targetColor;
            //}
        }
    }
}
