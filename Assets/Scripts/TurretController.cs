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

        [Header("Idle")]
        public float turretRotateSpeed = 25f;

        [Header("Overheat")]
        public int readyOverHeat = 0;
        public float overHeatTime = 10f;
        public bool overheatBool = false;

        [Header("Reloading")]
        public float reloadTime = 6f;
        public bool isReloading = false;

        [Header("Turret State Colors")]
        public Color overheatColor = Color.red;
        public Color idleColor = Color.green;
        public Color shootColor = Color.blue;
        public Color reloadColor = Color.yellow;

        [Header("Object Pooler")]
        public int bulletIndex;

        //Declare before Start()
        private void Awake()
        {
            currentAmmo = maxAmmo;  //Fill current Ammo
            fireCooldown = 0f;      //Reset fire Cooldown
            turretRenderer = GetComponent<Renderer>(); //Get turret redenderer for color change
        }

        private void Start()
        {
            SetupStateMachine();    //Setup State Machine
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

            //Shooting -> ?
            At(shootingState, overheatState, new FuncPredicate(() => overheatBool));
            At(shootingState, reloadingState, new FuncPredicate(() => currentAmmo <= 0));
            At(shootingState, idleState, new FuncPredicate(() => !IsTargetInRange()));

            //Overheat -> ?
            At(overheatState, shootingState, new FuncPredicate(() => !overheatBool && IsTargetInRange() && HasLineOfSight()));
            At(overheatState, reloadingState, new FuncPredicate(() => !overheatBool && currentAmmo <= 0));
            At(overheatState, idleState, new FuncPredicate(() => !overheatBool && !IsTargetInRange()));

            //Reloading -> ?
            At(reloadingState, shootingState, new FuncPredicate(() => !isReloading && currentAmmo > 0 && IsTargetInRange() && HasLineOfSight()));
            At(reloadingState, idleState, new FuncPredicate(() => currentAmmo > 0 && !IsTargetInRange()));

            // Set initial state
            stateMachine.SetState(idleState);
        }

        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition); //NOT USED DUE TO MULTIPLE DIFF REQUIREMENTS FOR TRANSITIONS
        private void Update()
        {
            stateMachine.Update();          //Constant State Machine Update for Use
            fireCooldown -= Time.deltaTime; //Fire Cooldown for each bullet shot
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate(); //Constant State Machine Fixed Update for Use
        }

        //Idle
        public void OnIdle()
        {
            //Random Turret Rotation to Simulate Idle State
            if (!IsTargetInRange())
            {
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
                turretBody.rotation = Quaternion.Slerp(turretBody.rotation, targetRotation, Time.deltaTime * turretRotateSpeed);
            }
        }

        //Tracking
        public bool IsTargetInRange()   //Detect IF Target is < Tracking Range
        {
            if (player == null) return false;
            return Vector3.Distance(transform.position, player.position) <= shootingRange;
        }

        public void TrackPlayer()       //IF Player is In Range, Move Turret To Look at Player
        {
            if (player == null || !IsTargetInRange()) return;

            Vector3 direction = player.position - turretBody.position;
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            turretBody.rotation = Quaternion.Slerp(turretBody.rotation, targetRotation, Time.deltaTime * 300f);
        }

        private bool HasLineOfSight()   //Using Raycaster to Detect If player is within Line Of Sight of the Player, this is for If there is an object Blocking the turret
        {
            if (player == null) return false;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit))
            {
                return hit.collider.CompareTag("Player");
            }
            return false;
        }

        //Shooting
        public void Shoot() 
        {
            if (fireCooldown > 0f || overheatBool || currentAmmo <= 0) return;  //Prevent Shooting when Overloading/Reloading

            FireBullet(bulletIndex);    //Use ObjectPooler Index of Bullet
            currentAmmo--;              //Decrease Current Ammo
            fireCooldown = fireRate;    //Control bullet shot speed
            readyOverHeat++;            //Ready Overheat for turret

            if (readyOverHeat >= 15)
            {
                overheatBool = true;
                StartOverheating();
            }
        }

        public void FireBullet(int bulletIndex)
        {
            var bullet = ObjectPooler.DequeueObject<BulletController>(bulletIndex);              //Object Pooler Dequeue Object
            if (bullet != null)
            {
                bullet.transform.position = firePoint.position; //Shoot from specific location
                bullet.gameObject.SetActive(true);              //Bullet is active

                Vector3 directionToPlayer = (player.position - firePoint.position).normalized;  //Find Player location
                bullet.Initialize(directionToPlayer);           //Shoot to direction of Player
            }
        }

        //Overheat
        public void StartOverheating()  //Start Overheat Timer
        {
            StartCoroutine(OverheatCoroutine());
        }   

        private IEnumerator OverheatCoroutine() //Using Set State here for easier state manuevour & fix bug faster :p
        {
            yield return new WaitForSeconds(overHeatTime);  //Overheat Timer
            overheatBool = false;   //Stop Overheat

            if ((IsTargetInRange() && HasLineOfSight()))
            {
                readyOverHeat += 1;  //Fix Bug that when Overheat Finishes readyOverheat is lost by 1 count
            }
            else if ((!IsTargetInRange() && !HasLineOfSight()))
            {
                stateMachine.SetState(new IdleState(this, animator));       //Back to Idle State
            }
            if (currentAmmo <= 0)
            {
                stateMachine.SetState(new ReloadingState(this, animator));  //Continue to Reload State
            }
        }

        //Reload
        public void Reload()    //Start Reload Timer
        {
            StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            isReloading = true;     //Stops other function when realoading
            yield return new WaitForSeconds(reloadTime);    //Reload Timer 
            readyOverHeat = 0;      //Reset Overheat Timer
            if ((IsTargetInRange() && HasLineOfSight()))
            {
                readyOverHeat += 1;  //Fix Bug that when Overheat Finishes readyOverheat is lost by 1 count
            }
            currentAmmo = maxAmmo;  //Refills Ammo
            isReloading = false;    //End Reloading
        }

        //State Color Change
        public void ChangeTurretColor(Color targetColor)    //Switch Color of Turret According To Curret State
        {
            if (turretRenderer != null)
            {
                turretRenderer.material.color = targetColor;    
            }
        }
    }
}


