using AshenForgotten.Combat;
using UnityEngine;

namespace AshenForgotten.Enemies
{
    public interface IEnemyBrain
    {
        void Init(EnemyContext ctx);
        void Tick(float dt);
        void OnDamaged(in HitInfo hit);
    }

    // Context passed to brains so they don't need to know the concrete controller
    public sealed class EnemyContext
    {
        public Transform Self;
        public Rigidbody2D Body;
        public Transform Player;
        public Transform EdgeCheck;
        public LayerMask GroundLayer;
        public IEnemyView View;
    }

    public interface IEnemyView
    {
        bool FacingRight { get; }
        void SetFacing(float dir);
    }
}
