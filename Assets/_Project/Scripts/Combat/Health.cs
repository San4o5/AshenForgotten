using System;
using UnityEngine;

namespace AshenForgotten.Combat
{
    public abstract class Health : MonoBehaviour, IDamageable, IHealth
    {
        [Header("Health")]
        [SerializeField] protected int _maxHealth = 100;
        [SerializeField] protected float _invincibilityDuration = 0.2f;

        public event Action<int, int> HealthChanged;
        public event Action Died;

        protected int _current;
        protected float _invincibleTimer;
        protected bool _isDead;

        public int Current => _current;
        public int Max => _maxHealth;
        public bool IsDead => _isDead;

        protected virtual void Awake()
        {
            _current = _maxHealth;
        }

        protected virtual void Start()
        {
            HealthChanged?.Invoke(_current, _maxHealth);
        }

        protected virtual void Update()
        {
            if (_invincibleTimer > 0f) _invincibleTimer -= Time.deltaTime;
        }

        public void TakeDamage(HitInfo hit)
        {
            if (_isDead) return;
            if (_invincibleTimer > 0f) return;
            if (hit.Damage <= 0) return;

            _current = Mathf.Max(0, _current - hit.Damage);
            _invincibleTimer = _invincibilityDuration;
            HealthChanged?.Invoke(_current, _maxHealth);

            OnDamageReceived(hit);

            if (_current == 0)
            {
                _isDead = true;
                OnDie(hit);
                Died?.Invoke();
            }
        }

        protected abstract void OnDamageReceived(in HitInfo hit);
        protected abstract void OnDie(in HitInfo hit);
    }
}
