using UnityEngine;

namespace AshenForgotten.Combat
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(DamageDealer))]
    public class AttackHitbox : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _slashVfx;

        private Collider2D _collider;
        private DamageDealer _damageDealer;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _damageDealer = GetComponent<DamageDealer>();
            _collider.enabled = false;
            if (_slashVfx != null) _slashVfx.enabled = false;
        }

        public void Activate()
        {
            _damageDealer.ResetHits();
            _collider.enabled = true;
            if (_slashVfx != null) _slashVfx.enabled = true;
        }

        public void Deactivate()
        {
            _collider.enabled = false;
            if (_slashVfx != null) _slashVfx.enabled = false;
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null) return;
            bool active = box.enabled;
            Gizmos.color = active ? new Color(1f, 0.2f, 0.2f, 0.6f) : new Color(1f, 1f, 0f, 0.25f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
            if (active) Gizmos.DrawCube(box.offset, box.size);
        }
    }
}
