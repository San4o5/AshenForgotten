using AshenForgotten.Combat;

namespace AshenForgotten.Enemies
{
    // Lets DamageDealer (Combat) push hits into the enemy Brain for aggro reactions.
    public interface IBrainHitNotifier
    {
        void NotifyBrainOfHit(in HitInfo hit);
    }
}
