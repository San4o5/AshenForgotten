using System.Collections.Generic;
using AshenForgotten.CameraSystem;
using AshenForgotten.Enemies;
using UnityEngine;

namespace AshenForgotten.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class DamageDealer : MonoBehaviour, IAttacker
    {
        [SerializeField] private int _damage = 20;
        [SerializeField] private LayerMask _targetLayers = ~0;
        [SerializeField] private float _knockbackForce = 0f;
        [SerializeField] private float _knockbackUp = 0f;
        [SerializeField] private Transform _origin;   // used for knockback direction; defaults to self

        [Header("Hit feedback")]
        [SerializeField] private float _hitstopDuration = 0.05f;
        [SerializeField] private float _shakeDuration = 0.1f;
        [SerializeField] private float _shakeAmplitude = 0.15f;

        private readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();

        public int Damage => _damage;
        public Vector2 Position => _origin != null ? (Vector2)_origin.position : (Vector2)transform.position;
        public float KnockbackForce => _knockbackForce;
        public float KnockbackUp => _knockbackUp;
        public GameObject Owner => gameObject;

        public void ResetHits()
        {
            _hitThisSwing.Clear();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

            // Ignore trigger volumes on the target (hurtboxes, sensors). Damage applies only to solid bodies.
            if (other.isTrigger) return;

            var target = other.GetComponentInParent<IDamageable>();
            if (target == null) return;
            if (target.IsDead) return;
            if (!_hitThisSwing.Add(target)) return;

            Vector2 targetPos = other.bounds.center;
            Vector2 dir = (targetPos - Position).sqrMagnitude > 0.0001f
                ? (targetPos - Position).normalized
                : Vector2.right;

            var hit = new HitInfo(_damage, Position, dir, _knockbackForce, _knockbackUp, gameObject);
            target.TakeDamage(hit);

            // Optional: also notify enemy brain about being hit (for aggro)
            var brainHolder = other.GetComponentInParent<IBrainHitNotifier>();
            brainHolder?.NotifyBrainOfHit(hit);

            Hitstop.Freeze(_hitstopDuration);
            if (CameraFollow.Instance != null)
                CameraFollow.Instance.Shake(_shakeDuration, _shakeAmplitude);

#if UNITY_EDITOR
            Debug.Log($"[DamageDealer] Hit {other.name} for {_damage}", other);
#endif
        }
    }
}
