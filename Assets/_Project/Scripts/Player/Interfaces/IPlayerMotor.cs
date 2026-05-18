using UnityEngine;

namespace AshenForgotten.Player
{
    public interface IPlayerMotor
    {
        Vector2 Velocity { get; }
        bool IsGrounded { get; }
        bool FacingRight { get; }

        void ApplyExternalVelocity(Vector2 v);
        void FreezeForDeath();
        void Flip();
    }
}
