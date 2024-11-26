using UnityEngine;

namespace Turret
{
    public class ReloadingState : IState
    {
        private TurretController turret;
        private Animator animator;

        public ReloadingState(TurretController turret, Animator animator)
        {
            this.turret = turret;
            this.animator = animator;
        }

        public void OnEnter()
        {
            animator?.SetBool("IsReloading", true);
            turret.ChangeTurretColor(turret.reloadColor);
            turret.Reload();
            Debug.Log("Entered Reloading State");
        }

        public void OnExit()
        {
            animator?.SetBool("IsReloading", false);
            Debug.Log("Exiting Reloading State");
        }

        public void Update() { }

        public void FixedUpdate() { }

    }
}
