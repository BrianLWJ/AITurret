using UnityEngine;

namespace Turret
{
    public class ShootingState : BaseState
    {
        public ShootingState(TurretController turret, Animator animator) : base(turret, animator) { }

        public override void OnEnter()
        {
            animator.CrossFade(ShootHash, crossFadeDuration);
        }

        public override void Update()
        {
            turret.HandleShoot();

            // Transition to OverheatState if overheated, or to ReloadingState if reload is needed.
            if (turret.IsOverheated())
            {
                turret.ChangeState(new OverheatState(turret, animator));
            }
            else if (turret.NeedsReloading())
            {
                turret.ChangeState(new ReloadingState(turret, animator));
            }
        }

        public override void OnExit()
        {
            // Reset any shooting-related states if necessary.
        }
    }
}
