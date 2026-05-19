using AshenForgotten.Combat;
using UnityEngine;

namespace AshenForgotten.Enemies
{
    public class EnemyDamageOnTouch : MonoBehaviour, IAttacker
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private float _hitCooldown = 0.6f;
        [SerializeField] private float _knockbackForce = 8f;
        [SerializeField] private float _knockbackUp = 4f;
        [SerializeField] private LayerMask _targetLayers = ~0;

        private float _lastHitTime = -999f;

        public int Damage => _damage;
        // Position points to the enemy root (parent), so knockback direction is from enemy body, not hurt zone
        public Vector2 Position => transform.parent != null ? (Vector2)transform.parent.position : (Vector2)transform.position;
        public float KnockbackForce => _knockbackForce;
        public float KnockbackUp => _knockbackUp;
        public GameObject Owner => gameObject;

        private void OnTriggerStay2D(Collider2D other) => TryDamage(other);

        private void TryDamage(Collider2D other)
        {
            if (Time.time - _lastHitTime < _hitCooldown) return;
            if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

            // Ignore trigger volumes (e.g. player's AttackHitbox / hurtboxes) — only solid body counts as "touch"
            if (other.isTrigger) return;

            var target = other.GetComponentInParent<IDamageable>();
            if (target == null || target.IsDead) return;

            Vector2 targetPos = other.bounds.center;
            Vector2 dir = (targetPos - Position).sqrMagnitude > 0.0001f
                ? (targetPos - Position).normalized
                : Vector2.right;

            var hit = new HitInfo(_damage, Position, dir, _knockbackForce, _knockbackUp, gameObject);
            target.TakeDamage(hit);
            _lastHitTime = Time.time;
        }
    }
}
