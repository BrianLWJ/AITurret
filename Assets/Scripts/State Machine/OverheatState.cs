using UnityEngine;

namespace Turret
{
    public class OverheatState : BaseState
    {
        public OverheatState(TurretController turret, Animator animator) : base(turret, animator) { }

        public override void OnEnter()
        {
            animator.CrossFade(OverheatHash, crossFadeDuration);
            turret.HandleOverheat();
        }

        public override void Update()
        {
            // Check if cooled down, then transition to IdleState.
            if (!turret.IsOverheated())
            {
                turret.ChangeState(new IdleState(turret, animator));
            }
        }

        public override void OnExit()
        {
            // Reset any overheating-specific states if necessary.
        }
    }
}
