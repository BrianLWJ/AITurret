using UnityEngine;

namespace Turret
{
    public class IdleState : IState
    {
        private TurretController turret;
        private Animator animator;

        public IdleState(TurretController turret, Animator animator)
        {
            this.turret = turret;
            this.animator = animator;
        }

        public void OnEnter()
        {
            animator?.SetBool("IsIdle", true);
            Debug.Log("Entered Idle State");
        }

        public void OnExit()
        {
            animator?.SetBool("IsIdle", false);
            Debug.Log("Exiting Idle State");
        }

        public void Update()
        {
            turret.OnIdle();
        }

        public void FixedUpdate() { }
    }
}
