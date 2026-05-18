using AshenForgotten.Combat;
using UnityEngine;

namespace AshenForgotten.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private float _attackCooldown = 0.3f;
        [SerializeField] private AttackHitbox _attackHitbox;

        private IPlayerInput _input;
        private Animator _animator;
        private float _attackTimer;
        private bool _enabled = true;

        private static readonly int HashAttack = Animator.StringToHash("Attack");

        public void SetEnabled(bool enabled) => _enabled = enabled;

        private void Awake()
        {
            _input = GetComponent<IPlayerInput>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!_enabled) return;
            if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;

            if (!_input.AttackPressed) return;
            if (_attackTimer > 0f) return;

            _attackTimer = _attackCooldown;
            _animator.SetTrigger(HashAttack);
        }

        // Called from HER_Attack animation event at active frame
        public void OnAttackHit()
        {
            if (_attackHitbox != null) _attackHitbox.Activate();
        }

        // Called from HER_Attack animation event at end frame
        public void OnAttackEnd()
        {
            if (_attackHitbox != null) _attackHitbox.Deactivate();
        }
    }
}
