using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int _maxHealth = 60;
    [SerializeField] private float _invincibilityDuration = 0.15f;
    [SerializeField] private float _destroyDelayAfterDie = 1.2f;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private int _currentHealth;
    private float _invincibleTimer;
    private bool _isDead;

    private Animator _animator;

    private static readonly int HashHurt = Animator.StringToHash("Hurt");
    private static readonly int HashDie  = Animator.StringToHash("Die");

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => _isDead;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _currentHealth = _maxHealth;
    }

    private void Update()
    {
        if (_invincibleTimer > 0f) _invincibleTimer -= Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;
        if (_invincibleTimer > 0f) return;
        if (damage <= 0) return;

        _currentHealth = Mathf.Max(0, _currentHealth - damage);
        _invincibleTimer = _invincibilityDuration;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth == 0) Die();
        else if (_animator != null) _animator.SetTrigger(HashHurt);
    }

    private void Die()
    {
        _isDead = true;
        if (_animator != null) _animator.SetTrigger(HashDie);

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;

        OnDied?.Invoke();
        Destroy(gameObject, _destroyDelayAfterDie);
    }
}
