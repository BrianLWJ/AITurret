using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

namespace Turret
{
    public class TurretController : MonoBehaviour
    {
        public Transform player;
        public float shootingRange = 10f;
        public float fireRate = 1f; // Fire rate in seconds
        public float reloadTime = 6f;
        public int maxAmmo = 10;
        private int currentAmmo;
        private float fireCooldown;
        public float overheatTime = 10f;
        public Color overheatColor = Color.red;
        private Color originalColor;
        public float fadeDuration = 1f;
        public Transform turretBody;
        public float overHeatTime = 10f;
        private Animator animator;
        private StateMachine stateMachine;
        public int readyOverHeat = 5;

        public Transform firePoint;
        public string bulletType = "StandardBullet"; // Specify the bullet type by name here
        private Renderer turretRenderer;

        private void Awake()
        {
            currentAmmo = maxAmmo;
            fireCooldown = 0f;
            turretRenderer = GetComponent<Renderer>();
            originalColor = turretRenderer.material.color;
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
            At(reloadingState, shootingState, new FuncPredicate(() => IsTargetInRange(shootingRange) && HasLineOfSight()));
            At(shootingState, overheatState, new FuncPredicate(IsOverheated));
            At(overheatState, idleState, new FuncPredicate(() => !IsTargetInRange(shootingRange) && !HasLineOfSight()));

            Any(idleState, new FuncPredicate(ReturnToIdleState));

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

        public void OnShoot()
        {
            if (currentAmmo > 0 && fireCooldown <= 0f)
            {
                FireBullet();
                currentAmmo--;
                fireCooldown = fireRate; // Reset fire cooldown
                readyOverHeat++;

                if (readyOverHeat >= 5)
                {
                    StartOverheating();

                }
            }

            else if (currentAmmo <= 0)
            {
                Reload();
            }

        }

        public void FireBullet()
        {
            BulletController bullet = ObjectPooler.DequeueObject<BulletController>(bulletType);
            if (bullet != null)
            {
                // Rotate turret towards player
                Vector3 direction = player.position - turretBody.position;
                direction.y = 0; // Keep the rotation on the horizontal plane
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                turretBody.rotation = Quaternion.Slerp(turretBody.rotation, targetRotation, Time.deltaTime * 300f);


                // Position bullet at the fire point and initialize it
                bullet.transform.position = firePoint.position;
                bullet.gameObject.SetActive(true);

                Vector3 directionToPlayer = (player.position - firePoint.position).normalized;
                bullet.Initialize(directionToPlayer);
            }
        }

        public void Reload()
        {
            StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            yield return new WaitForSeconds(reloadTime);
            currentAmmo = maxAmmo;
        }

//OVERHEAT
        public void StartOverheating()
        {
            turretRenderer.material.DOColor(overheatColor, fadeDuration);
            StartCoroutine(OverHeatCoroutine());
        }

        public void StopOverheating()
        {
            turretRenderer.material.DOColor(originalColor, fadeDuration);
            StartCoroutine(OverHeatCoroutine());
        }
        private IEnumerator OverHeatCoroutine()
        {
            yield return new WaitForSeconds(overHeatTime);
        }

        private bool IsTargetInRange(float range)
        {
            if (player == null) return false;
            return Vector3.Distance(transform.position, player.position) <= range;
        }

        public bool NeedsReloading()
        {
            return currentAmmo <= 0;
        }

        private bool HasLineOfSight()
        {
            if (player == null) return false;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("Player Hit");
                    return true;
                }
            }
            return false;
        }

        private bool IsOverheated()
        {
            return false; // Placeholder for actual overheating logic
        }

        private bool ReturnToIdleState()
        {
            return false;
        }

        public void OnIdle()
        {
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

            //random direction turn
            Quaternion targetRotation = Quaternion.LookRotation(randomDirection);

            //rotate the turret body
            turretBody.rotation = Quaternion.Slerp(turretBody.rotation, targetRotation, Time.deltaTime * 2f); // Adjust the speed as needed
        }
    }
}
