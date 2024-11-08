using UnityEngine;

namespace Turret
{
    public class IdleState : BaseState
    {
        public IdleState(TurretController turret, Animator animator) : base(turret, animator) { }

        public override void OnEnter()
        {
            animator.CrossFade(IdleHash, crossFadeDuration);
        }

        public override void Update()
        {
            // Check if the target is within range, and if so, transition to ShootingState.
            if (turret.IsTargetInRange(turret.shootingRange) && turret.HasLineOfSight())
            {
                turret.ChangeState(new ShootingState(turret, animator));
            }
        }

        public override void OnExit()
        {
            // Reset any idle-specific states if necessary.
        }
    }
}
