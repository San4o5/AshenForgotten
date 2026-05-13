using UnityEngine;

public class EnemyDamageOnTouch : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _hitCooldown = 0.6f;

    private float _lastHitTime = -999f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamage(collision.collider);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (Time.time - _lastHitTime < _hitCooldown) return;

        var health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;
        if (health.IsDead) return;

        _lastHitTime = Time.time;
        health.TakeDamageFrom(_damage, transform.position);
    }
}
