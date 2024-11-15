using UnityEngine;

namespace Turret
{
    public class ShootingState : IState
    {
        private TurretController turret;
        private Animator animator;

        public ShootingState(TurretController turret, Animator animator)
        {
            this.turret = turret;
            this.animator = animator;
        }

        public void OnEnter()
        {
            animator?.SetBool("IsShooting", true);
            Debug.Log("Entered Shooting State");
        }

        public void OnExit()
        {
            animator?.SetBool("IsShooting", false);
            Debug.Log("Exiting Shooting State");
        }

        public void Update()
        {
            turret.OnShoot();
        }

        public void FixedUpdate() { }
    }
}
