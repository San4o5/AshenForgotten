using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageDealer : MonoBehaviour
{
    [SerializeField] private int _damage = 20;
    [SerializeField] private LayerMask _targetLayers = ~0;

    private readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();

    public void ResetHits()
    {
        _hitThisSwing.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;
        if (!_hitThisSwing.Add(target)) return;

        target.TakeDamage(_damage);
        Debug.Log($"[DamageDealer] Hit {other.name} for {_damage} dmg", other);
    }
}
