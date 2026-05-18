namespace AshenForgotten.Combat
{
    public interface IKnockbackReceiver
    {
        void ApplyKnockback(in HitInfo hit);
    }
}
