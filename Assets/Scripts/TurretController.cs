//using UnityEngine;

//public class Turret : MonoBehaviour
//{
//    public enum TurretState { Idle, Tracking, Shooting, Reloading }
//    public TurretState currentState;

//    public Transform player;
//    public Transform firePoint;
//    public float trackingRange = 10f;
//    public float shootInterval = 0.5f;
//    public int burstCount = 3;
//    public float reloadTime = 2f;

//    private int shotsFired = 0;
//    private float stateTimer = 0f;

//    void Start()
//    {
//        SetState(TurretState.Idle);
//    }

//    void Update()
//    {
//        switch (currentState)
//        {
//            case TurretState.Idle:
//                if (Vector3.Distance(transform.position, player.position) <= trackingRange)
//                {
//                    SetState(TurretState.Tracking);
//                }
//                break;

//            case TurretState.Tracking:
//                TrackPlayer();
//                if (Vector3.Distance(transform.position, player.position) > trackingRange)
//                {
//                    SetState(TurretState.Idle);
//                }
//                else if (stateTimer <= 0f)
//                {
//                    SetState(TurretState.Shooting);
//                }
//                break;

//            case TurretState.Shooting:
//                Shoot();
//                if (shotsFired >= burstCount)
//                {
//                    SetState(TurretState.Reloading);
//                }
//                break;

//            case TurretState.Reloading:
//                if (stateTimer <= 0f)
//                {
//                    SetState(TurretState.Tracking);
//                }
//                break;
//        }

//        stateTimer -= Time.deltaTime;
//    }

//    void SetState(TurretState newState)
//    {
//        currentState = newState;

//        switch (currentState)
//        {
//            case TurretState.Idle:
//                break;

//            case TurretState.Tracking:
//                stateTimer = 1f; // Delay before shooting
//                break;

//            case TurretState.Shooting:
//                stateTimer = shootInterval;
//                shotsFired = 0;
//                break;

//            case TurretState.Reloading:
//                stateTimer = reloadTime;
//                break;
//        }
//    }

//    void TrackPlayer()
//    {
//        Vector3 direction = (player.position - transform.position).normalized;
//        Quaternion lookRotation = Quaternion.LookRotation(direction);
//        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
//    }

//    void Shoot()
//    {
//        if (stateTimer <= 0f)
//        {
//            GameObject bullet = ObjectPooler.DequeueObject<BulletController>("Bullet").gameObject;
//            bullet.transform.position = firePoint.position;
//            bullet.transform.rotation = firePoint.rotation;
//            bullet.SetActive(true);

//            shotsFired++;
//            stateTimer = shootInterval;
//        }
//    }
//}

//////////////////////////////////////////////////////////////////////////////////////////////////////
//using UnityEngine;
//using System.Collections;
//using Turret;

//namespace Turret
//{
//    public class TurretController : MonoBehaviour
//    {
//        public float overheatTime = 5f;
//        public float reloadTime = 2f;
//        public float shootInterval = 0.5f;       // Interval between bursts
//        public float trackingRange = 15f;        // Range for starting to track the player
//        public float shootingRange = 10f;        // Range for shooting the player
//        public int burstCount = 3;               // Number of shots in each burst
//        public float burstInterval = 0.2f;       // Time between each shot in a burst
//        public Transform Player;                 // Player target
//        public GameObject bulletPrefab;
//        public Transform firePoint;

//        private Animator animator;
//        private StateMachine stateMachine;
//        public bool isShooting = false;
//        private float overheatTimer = 0f;
//        private bool isOverheated = false;
//        public bool burstCompleted => !isShooting; //X//

//        // Define states
//        public IdleState idleState;
//        public TrackingState trackingState;
//        public ShootingState shootingState;
//        public ReloadingState reloadingState;
//        public OverheatState overheatState;

//        void Awake()
//        {
//            animator = GetComponent<Animator>();

//////////////////////////////////////////////////////////////////////////////////////////////////////
//            //State Machine

//            stateMachine = new StateMachine();

//            // Declare states
//            var idleState = new IdleState(turret: this, animator);
//            var trackingState = new TrackingState(turret: this, animator);
//            var shootingState = new ShootingState(turret: this, animator);
//            var reloadingState = new ReloadingState(turret: this, animator);
//            var overheatState = new OverheatState(turret: this, animator);

//            //stateMachine.AddState(idleState);
//            //stateMachine.AddState(trackingState);
//            //stateMachine.AddState(shootingState);
//            //stateMachine.AddState(reloadingState);
//            //stateMachine.AddState(overheatState);

//            // Define transitions
//            At(idleState,       trackingState,  new FuncPredicate(() => Player != null && IsTargetInRange(trackingRange)));
//            At(trackingState,   shootingState,  new FuncPredicate(() => IsTargetInRange(shootingRange) && HasLineOfSight()));
//            At(shootingState,   overheatState,  new FuncPredicate(() => IsOverheated()));
//            At(overheatState,   idleState,      new FuncPredicate(() => !IsOverheated()));
//            At(shootingState,   reloadingState, new FuncPredicate(() => NeedsReloading()));
//            At(reloadingState,  shootingState,  new FuncPredicate(() => !NeedsReloading()));

//            //Set Initial State
//            stateMachine.SetState(idleState);
//        }

//        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
//        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
//////////////////////////////////////////////////////////////////////////////////////////////////////

//        void Update()
//        {
//            stateMachine.Update();
//            ManageOverheat();
//        }

//        void FixedUpdate()
//        {
//            stateMachine.FixedUpdate();
//        }

//        private bool IsTargetInRange(float range)
//        {
//            if (Player == null) return false;
//            float distance = Vector3.Distance(transform.position, Player.position);
//            return distance <= range;
//        }

//        private bool IsOverheated()
//        {
//            return isOverheated;
//        }

//        private void ManageOverheat()
//        {
//            if (isShooting)
//            {
//                overheatTimer += Time.deltaTime;
//                if (overheatTimer >= overheatTime)
//                {
//                    isOverheated = true;
//                    overheatTimer = 0f;
//                }
//            }
//            else if (isOverheated)
//            {
//                overheatTimer += Time.deltaTime;
//                if (overheatTimer >= reloadTime)
//                {
//                    isOverheated = false;
//                    overheatTimer = 0f;
//                }
//            }
//        }

//        private bool NeedsReloading()
//        {
//            // Check if reload is needed
//            return false;
//        }

//        public void TriggerTransition(IState newState)
//        {
//            stateMachine.SetState(newState);
//        }

//        public void Shoot()
//        {
//            if (!isShooting)
//            {
//                StartCoroutine(ShootBurst());
//            }
//        }

//        private IEnumerator ShootBurst()
//        {
//            isShooting = true;

//            for (int i = 0; i < burstCount; i++)
//            {
//                Fire();
//                yield return new WaitForSeconds(burstInterval);
//            }

//            isShooting = false;
//        }

//        private void Fire()
//        {
//            if (firePoint != null && bulletPrefab != null)
//            {
//                Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
//            }
//        }

//        private bool HasLineOfSight()
//        {
//            RaycastHit hit;
//            Vector3 directionToPlayer = (Player.position - transform.position).normalized;

//            if (Physics.Raycast(transform.position, directionToPlayer, out hit, shootingRange))
//            {
//                return hit.transform == Player;
//            }
//            return false;
//        }
//    }
//}

using UnityEngine;

namespace Turret
{
    public class TurretController : MonoBehaviour
    {
        public Transform Player;
        public Transform firePoint; // The fire point from where the bullet is fired
        public Transform barrel;     // The barrel's position (can be used for precise bullet spawning)
        public Transform head;       // The head or direction from where the bullet will travel (for velocity)
        public float trackingRange = 15f;
        public float shootingRange = 10f;
        public float shootInterval = 0.5f;
        public int magazine = 3;
        public float reloadTime = 2f;
        public float overheatThreshold = 10f;
        public float cooldownTime = 3f;
        public float projSpeed = 20f; // Projectile speed
        public LayerMask obstacleMask;

        private float overheatTimer = 0f;
        private int shotsFired = 0;
        private float nextShootTime = 0f;

        public Animator animator;
        //private StateMachine stateMachine;
        private IState currentState;
        void Awake()
        {
            //animator = GetComponent<Animator>();

            //// Initialize the state machine and states
            //stateMachine = new StateMachine();

            //var idleState = new IdleState(this, animator);
            //var shootingState = new ShootingState(this, animator);
            //var reloadingState = new ReloadingState(this, animator);
            //var overheatState = new OverheatState(this, animator);

            //// State transitions
            //At(idleState, shootingState, new FuncPredicate(() => IsTargetInRange(shootingRange) && HasLineOfSight()));
            //At(shootingState, overheatState, new FuncPredicate(() => IsOverheated()));
            //At(overheatState, idleState, new FuncPredicate(() => !IsOverheated()));
            //At(shootingState, reloadingState, new FuncPredicate(() => NeedsReloading()));
            //At(reloadingState, shootingState, new FuncPredicate(() => !NeedsReloading()));

            //stateMachine.SetState(idleState);

            animator = GetComponent<Animator>();
            currentState = new IdleState(this, animator);
            Player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void Start()
        {
            Player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void Update()
        {
            stateMachine.Update();
        }

        void FixedUpdate()
        {
            TrackTarget(); // Tracks the target constantly
            stateMachine.FixedUpdate();
        }

        public void ChangeState(IState newState)
        {
            currentState?.OnExit();
            currentState = newState;
            currentState.OnEnter();
        }   

        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);

        public void HandleShoot()
        {
            Debug.Log("handle shooting");
            if (!IsOverheated()) //  if (cooldownTime <= 0 && !IsOverheated())
            {
                cooldownTime = 0;
                Shoot();
                Debug.Log("Shoot Phase 2");
                shotsFired++;
                overheatTimer += shootInterval;

                if (shotsFired >= magazine)
                {
                    StartReloading();
                }
            }
        }

        void Shoot()
        {
            Debug.Log("Shoot Phase 1");
            // Get a bullet from the object pool instead of instantiating a new one
            GameObject bullet = ObjectPooler.DequeueObject<BulletController>("Bullet").gameObject;

            // Set bullet's position and rotation
            bullet.transform.position = barrel.position;
            bullet.transform.rotation = head.rotation;

            Debug.Log("SHOOTING");

            // Initialize the bullet towards the player
            bullet.GetComponent<BulletController>().Initialize(Player.position);
        }


        public void StartReloading()
        {
            shotsFired = 0;
            nextShootTime = Time.time + reloadTime;
        }

        public void HandleOverheat()
        {
            if (IsOverheated())
            {
                overheatTimer = 0f;
            }
        }

        void TrackTarget()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

            // Check if the player is within tracking range
            if (distanceToPlayer <= trackingRange)
            {
                // Rotate the turret's head to face the player
                Vector3 directionToPlayer = (Player.position - head.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                head.rotation = Quaternion.Slerp(head.rotation, lookRotation, Time.deltaTime * 5f);

                // Only shoot if within shooting range, has line of sight, and is allowed by shoot interval
                if (distanceToPlayer <= shootingRange && HasLineOfSight() && Time.time >= nextShootTime)
                {
                    nextShootTime = Time.time + shootInterval; // Set the next allowed shoot time
                    HandleShoot();
                }
            }
        }

        public bool IsTargetInRange(float range)
        {
            return Player != null && Vector3.Distance(transform.position, Player.position) <= range;
        }

        public bool HasLineOfSight()
        {
            if (Player == null) return false;

            Vector3 directionToTarget = (Player.position - firePoint.position).normalized;
            float distanceToTarget = Vector3.Distance(firePoint.position, Player.position);

            if (Physics.Raycast(firePoint.position, directionToTarget, out RaycastHit hit, distanceToTarget, obstacleMask))
            {
                return false;
            }

            return true;
        }

        public bool IsOverheated()
        {
            cooldownTime = 6f;
            return overheatTimer >= overheatThreshold;
        }

        public bool NeedsReloading()
        {
            cooldownTime = 3f;
            return shotsFired >= magazine;
        }
    }
}




