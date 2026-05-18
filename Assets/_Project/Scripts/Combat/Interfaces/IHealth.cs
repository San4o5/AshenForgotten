using System;

namespace AshenForgotten.Combat
{
    public interface IHealth
    {
        int Current { get; }
        int Max { get; }
        bool IsDead { get; }
        event Action<int, int> HealthChanged;
        event Action Died;
    }
}
