using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int _maxHealth = 5;
    [SerializeField] private float _invincibilityDuration = 0.5f;

    [Header("Knockback")]
    [SerializeField] private float _knockbackForce = 8f;
    [SerializeField] private float _knockbackUp = 4f;
    [SerializeField] private float _knockbackLockTime = 0.2f;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private int _currentHealth;
    private float _invincibleTimer;
    private bool _isDead;

    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerController _controller;

    private static readonly int HashHurt = Animator.StringToHash("Hurt");
    private static readonly int HashDie  = Animator.StringToHash("Die");

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => _isDead;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _controller = GetComponent<PlayerController>();
        _currentHealth = _maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
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
        else _animator.SetTrigger(HashHurt);
    }

    // Damage source location passed separately so knockback direction can be computed
    public void TakeDamageFrom(int damage, Vector2 sourcePosition)
    {
        if (_isDead || _invincibleTimer > 0f) return;
        ApplyKnockback(sourcePosition);
        TakeDamage(damage);
    }

    private void ApplyKnockback(Vector2 sourcePosition)
    {
        if (_rb == null) return;
        float dirX = Mathf.Sign(transform.position.x - sourcePosition.x);
        if (dirX == 0f) dirX = 1f;
        _rb.linearVelocity = new Vector2(dirX * _knockbackForce, _knockbackUp);
        if (_controller != null) _controller.LockControl(_knockbackLockTime);
    }

    private void Die()
    {
        _isDead = true;
        _animator.SetTrigger(HashDie);
        if (_controller != null) _controller.SetControlEnabled(false);
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            // Freeze X so enemy bodies can't push the corpse sideways
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }
        OnDied?.Invoke();
    }

    // Called from HER_Die animation event on last frame (optional)
    public void OnDieAnimEnd()
    {
        // Hook for respawn / game over later
    }
}
