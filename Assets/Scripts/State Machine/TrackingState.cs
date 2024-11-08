//using UnityEngine;

//namespace Turret
//{
//    public class TrackingState : BaseState
//    {
//        public TrackingState(TurretController turret, Animator animator) : base(turret, animator) { }

//        public override void OnEnter()
//        {
//            animator.CrossFade(TrackingHash, crossFadeDuration);
//        }

//        public override void FixedUpdate()
//        {
//            turret.TrackTarget(); // Method to rotate towards the target
//        }

//        public override void OnExit()
//        {
//            Debug.Log("Exiting Tracking State");
//        }
//    }
//}

