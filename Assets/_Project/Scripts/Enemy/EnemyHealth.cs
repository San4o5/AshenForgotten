using AshenForgotten.Combat;
using UnityEngine;

namespace AshenForgotten.Enemies
{
    public class EnemyHealth : Health
    {
        [Header("Death")]
        [SerializeField] private float _destroyDelayAfterDie = 1.2f;

        [Header("Knockback on hit (0 = immune)")]
        [SerializeField] private float _hurtKnockbackForce = 3f;
        [SerializeField] private float _hurtKnockbackUp = 2f;

        private Animator _animator;
        private Rigidbody2D _rb;

        private static readonly int HashHurt = Animator.StringToHash("Hurt");
        private static readonly int HashDie  = Animator.StringToHash("Die");

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
        }

        protected override void OnDamageReceived(in HitInfo hit)
        {
            if (_animator != null) _animator.SetTrigger(HashHurt);

            if (_rb != null && _rb.bodyType == RigidbodyType2D.Dynamic && _hurtKnockbackForce > 0f)
            {
                float dirX = Mathf.Sign(transform.position.x - hit.SourcePosition.x);
                if (dirX == 0f) dirX = hit.Direction.x >= 0f ? 1f : -1f;
                _rb.linearVelocity = new Vector2(dirX * _hurtKnockbackForce, _hurtKnockbackUp);
            }
        }

        protected override void OnDie(in HitInfo hit)
        {
            if (_animator != null) _animator.SetTrigger(HashDie);

            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;

            Destroy(gameObject, _destroyDelayAfterDie);
        }
    }
}
