using UnityEngine;
using System.Collections;

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

        [Header("Turret")]
        public int currentAmmo;
        public int maxAmmo = 30;
        public float fireRate = 1f;
        private float fireCooldown;

        [Header("Reloading")]
        public float reloadTime = 6f;
        public bool isReloading = false;

        [Header("Overheat")]
        public int readyOverHeat = 0;
        public float overHeatTime = 10f;
        public bool overheatBool = false;

        [Header("Turret State Colors")]
        public Color overheatColor = Color.red;
        public Color idleColor = Color.green;
        public Color shootColor = Color.blue;
        public Color reloadColor = Color.yellow;

        public int bulletIndex;

        private void Awake()
        {
            currentAmmo = maxAmmo;
            fireCooldown = 0f;
            turretRenderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            SetupStateMachine();
        }

        private void SetupStateMachine()
        {
            stateMachine = new StateMachine();

            // Define states
            var idleState = new IdleState(this, animator);
            var shootingState = new ShootingState(this, animator);
            var reloadingState = new ReloadingState(this, animator);
            var overheatState = new OverheatState(this, animator);

            // Define transitions
            //Idle -> ?
            At(idleState, shootingState, new FuncPredicate(() => IsTargetInRange() && HasLineOfSight()));

            //Shooting
            At(shootingState, overheatState, new FuncPredicate(() => overheatBool));
            At(shootingState, reloadingState, new FuncPredicate(() => currentAmmo <= 0));
            At(shootingState, idleState, new FuncPredicate(() => !IsTargetInRange()));

            //Overheat
            At(overheatState, shootingState, new FuncPredicate(() => !overheatBool && IsTargetInRange() && HasLineOfSight()));
            At(overheatState, reloadingState, new FuncPredicate(() => !overheatBool && currentAmmo <= 0));
            At(overheatState, idleState, new FuncPredicate(() => !overheatBool && !IsTargetInRange()));

            //Reloading
            At(reloadingState, shootingState, new FuncPredicate(() => !isReloading && currentAmmo > 0 && IsTargetInRange() && HasLineOfSight()));
            At(reloadingState, idleState, new FuncPredicate(() => currentAmmo > 0 && !IsTargetInRange()));

            // Set the initial state
            stateMachine.SetState(idleState);
        }

        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);

        private void Update()
        {
            stateMachine.Update();
            fireCooldown -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        public void OnIdle()
        {
            turretRenderer.material.color = idleColor;
            if (!IsTargetInRange())
            {
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
                turretBody.rotation = Quaternion.Slerp(turretBody.rotation, targetRotation, Time.deltaTime * 25f);
            }
        }

        public bool IsTargetInRange()
        {
            if (player == null) return false;
            return Vector3.Distance(transform.position, player.position) <= shootingRange;
        }

        public void TrackPlayer()
        {
            if (player == null || !IsTargetInRange()) return;

            Vector3 direction = player.position - turretBody.position;
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            turretBody.rotation = Quaternion.Slerp(turretBody.rotation, targetRotation, Time.deltaTime * 300f);
        }

        private bool HasLineOfSight()
        {
            if (player == null) return false;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit))
            {
                return hit.collider.CompareTag("Player");
            }
            return false;
        }

        public void Shoot()
        {
            if (fireCooldown > 0f || overheatBool || currentAmmo <= 0) return;

            FireBullet(bulletIndex);
            currentAmmo--;
            fireCooldown = fireRate;
            readyOverHeat++;

            if (readyOverHeat >= 15)
            {
                overheatBool = true;
                StartOverheating();
            }
        }

        public void FireBullet(int bulletName)
        {
            var bullet = ObjectPooler.DequeueObject<BulletController>(bulletName);
            if (bullet != null)
            {
                bullet.transform.position = firePoint.position;
                bullet.gameObject.SetActive(true);

                Vector3 directionToPlayer = (player.position - firePoint.position).normalized;
                bullet.Initialize(directionToPlayer);
            }
        }

        public void StartOverheating()
        {
            StartCoroutine(OverheatCoroutine());
        }

        private IEnumerator OverheatCoroutine()
        {
            yield return new WaitForSeconds(overHeatTime);
            overheatBool = false;

            if ((IsTargetInRange() && HasLineOfSight()))
            {
                readyOverHeat = 1;  //Fix Bug that when Overheat Finishes readyOverheat is lost by 1 count
            }
            else if ((!IsTargetInRange() && !HasLineOfSight()))
            {
                stateMachine.SetState(new IdleState(this, animator));
            }
            if (currentAmmo <= 0)
            {
                stateMachine.SetState(new ReloadingState(this, animator));
            }
        }

        public void Reload()
        {
            StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            isReloading = true;
            yield return new WaitForSeconds(reloadTime);
            readyOverHeat = 0;
            currentAmmo = maxAmmo;
            isReloading = false;
        }

        public void ChangeTurretColor(Color targetColor)
        {
            if (turretRenderer != null)
            {
                turretRenderer.material.color = targetColor;
            }
        }
    }
}


