namespace AshenForgotten.Combat
{
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(HitInfo hit);
    }
}
