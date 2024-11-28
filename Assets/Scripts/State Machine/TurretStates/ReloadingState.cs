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
            Debug.Log("Entered Reloading State");
            turret.Reload();
        }

        public void OnExit()
        {
            animator?.SetBool("IsReloading", false);
            Debug.Log("Exiting Reloading State");
        }

        public void Update()
        {
            turret.TrackPlayer();
        }

        public void FixedUpdate() { }

    }
}
