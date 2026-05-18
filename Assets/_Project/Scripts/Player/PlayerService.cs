using AshenForgotten.Combat;
using UnityEngine;

namespace AshenForgotten.Player
{
    public static class PlayerService
    {
        public static Transform PlayerTransform { get; private set; }
        public static IDamageable PlayerDamageable { get; private set; }
        public static IHealth PlayerHealth { get; private set; }

        public static bool IsAvailable => PlayerTransform != null;

        public static void Register(Transform t, IDamageable damageable, IHealth health)
        {
            PlayerTransform = t;
            PlayerDamageable = damageable;
            PlayerHealth = health;
        }

        public static void Unregister(Transform t)
        {
            if (PlayerTransform != t) return;
            PlayerTransform = null;
            PlayerDamageable = null;
            PlayerHealth = null;
        }
    }
}
