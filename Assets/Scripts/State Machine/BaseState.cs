using UnityEngine;

namespace Turret
{
    public abstract class BaseState : IState
    {
        protected TurretController turretController;
        protected Animator animator;

        protected static readonly int HashIdle = Animator.StringToHash("Idle");
        protected static readonly int HashShooting = Animator.StringToHash("Shooting");
        protected static readonly int HashReloading = Animator.StringToHash("Reloading");
        protected static readonly int HashOverheat = Animator.StringToHash("Overheat");

        protected const float crossFadeDuration = 0.1f;

        // Corrected constructor parameter names to match field names
        public BaseState(TurretController turretController, Animator animator)
        {
            this.turretController = turretController;
            this.animator = animator;
        }

        public virtual void OnEnter()
        {
            // no-op
        }

        public virtual void Update()
        {
            // no-op
        }

        public virtual void FixedUpdate()
        {
            // no-op
        }

        public virtual void OnExit()
        {
            // no-op
        }
    }
}
