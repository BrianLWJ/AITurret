using UnityEngine;

namespace Turret
{
    public abstract class BaseState : IState
    {
        protected readonly TurretController turret;
        protected readonly Animator animator;

        // Define hashes for turret-specific animation states
        protected static readonly int IdleHash = Animator.StringToHash("Idle");
        protected static readonly int ShootHash = Animator.StringToHash("Shoot");
        protected static readonly int ReloadHash = Animator.StringToHash("Reload");
        protected static readonly int OverheatHash = Animator.StringToHash("Overheat");
        //protected static readonly int TrackingHash = Animator.StringToHash("Tracking");
        //
        protected const float crossFadeDuration = 0.1f;
        protected BaseState(TurretController turret, Animator animator)
        {
            this.turret = turret;
            this.animator = animator;
        }

        public virtual void OnEnter()
        {
            //noop
            // Implement ??? when entering a state
        }

        public virtual void Update()
        {
            //noop
            // Implement ??? per-frame for this state
        }

        public virtual void FixedUpdate()
        {
            //noop 
            // Implement ??? physics-related for this state
        }

        public virtual void OnExit()
        {
            //noop
            // Implement ???  when exiting a state
        }
    }
}

