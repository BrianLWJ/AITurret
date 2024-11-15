using UnityEngine;

namespace Turret
{
    public interface IState
    {
        void OnEnter();
        void Update();
        void FixedUpdate();
        void OnExit();
    }
}