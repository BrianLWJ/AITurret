using UnityEngine;

namespace Turret
{
    public class OverheatState : IState
    {
        private TurretController turret;
        private Animator animator;

        public OverheatState(TurretController turret, Animator animator)
        {
            this.turret = turret;
            this.animator = animator;
        }

        public void OnEnter()
        {
            animator?.SetBool("IsOverheated", true);
            turret.StartOverheating();
            Debug.Log("Entered Overheat State");
        }

        public void OnExit()
        {
            animator?.SetBool("IsOverheated", false);
            turret.StopOverheating();
            Debug.Log("Exiting Overheat State");
        }

        public void Update()
        {
            // Could add logic for cooling down
        }

        public void FixedUpdate() { }
    }
}
