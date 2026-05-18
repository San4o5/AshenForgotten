using UnityEngine;

namespace AshenForgotten.Combat
{
    public interface IAttacker
    {
        int Damage { get; }
        Vector2 Position { get; }
        float KnockbackForce { get; }
        float KnockbackUp { get; }
        GameObject Owner { get; }
    }
}
