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
            turret.ChangeTurretColor(turret.overheatColor);
            Debug.Log("Entered Overheat State");
        }

        public void OnExit()
        {
            animator?.SetBool("IsOverheated", false);
            Debug.Log("Exiting Overheat State");
        }

        public void Update()
        {
            turret.StartOverheating();
        }

        public void FixedUpdate() { }
    }
}
