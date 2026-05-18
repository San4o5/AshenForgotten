using UnityEngine;

namespace AshenForgotten.Combat
{
    public readonly struct HitInfo
    {
        public readonly int Damage;
        public readonly Vector2 SourcePosition;
        public readonly Vector2 Direction;
        public readonly float KnockbackForce;
        public readonly float KnockbackUp;
        public readonly GameObject Attacker;

        public HitInfo(int damage, Vector2 sourcePosition, Vector2 direction,
            float knockbackForce = 0f, float knockbackUp = 0f, GameObject attacker = null)
        {
            Damage = damage;
            SourcePosition = sourcePosition;
            Direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            KnockbackForce = knockbackForce;
            KnockbackUp = knockbackUp;
            Attacker = attacker;
        }
    }
}
