using UnityEngine;

namespace Turret
{
    public class ReloadingState : BaseState
    {
        public ReloadingState(TurretController turret, Animator animator) : base(turret, animator) { }

        public override void OnEnter()
        {
            animator.CrossFade(ReloadHash, crossFadeDuration);
            turret.StartReloading();
        }

        public override void Update()
        {
            // Check if the reload time has passed, then transition to ShootingState.
            if (Time.time >= turret.nextShootTime)
            {
                turret.ChangeState(new ShootingState(turret, animator));
            }
        }

        public override void OnExit()
        {
            // Reset any reloading-specific states if necessary.
        }
    }
}
