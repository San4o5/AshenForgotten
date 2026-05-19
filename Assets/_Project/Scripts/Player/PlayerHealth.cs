using AshenForgotten.Combat;
using UnityEngine;

namespace AshenForgotten.Player
{
    public class PlayerHealth : Health, IKnockbackReceiver
    {
        [Header("Knockback (defaults applied when HitInfo carries none)")]
        [SerializeField] private float _defaultKnockbackForce = 8f;
        [SerializeField] private float _defaultKnockbackUp = 5f;
        [SerializeField] private float _knockbackLockTime = 0.2f;

        private Animator _animator;
        private IPlayerControl _control;
        private IPlayerMotor _motor;

        private static readonly int HashHurt = Animator.StringToHash("Hurt");
        private static readonly int HashDie  = Animator.StringToHash("Die");

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            _control = GetComponent<IPlayerControl>();
            _motor = GetComponent<IPlayerMotor>();
        }

        protected override void OnDamageReceived(in HitInfo hit)
        {
            ApplyKnockback(in hit);
            if (_animator != null) _animator.SetTrigger(HashHurt);
        }

        protected override void OnDie(in HitInfo hit)
        {
            if (_animator != null) _animator.SetTrigger(HashDie);
            _control?.SetControlEnabled(false);
            _motor?.FreezeForDeath();
        }

        public void ApplyKnockback(in HitInfo hit)
        {
            if (_motor == null) return;

            float force = hit.KnockbackForce > 0f ? hit.KnockbackForce : _defaultKnockbackForce;
            float up    = hit.KnockbackUp    > 0f ? hit.KnockbackUp    : _defaultKnockbackUp;

            float dirX = Mathf.Sign(transform.position.x - hit.SourcePosition.x);
            if (dirX == 0f) dirX = 1f;

            _motor.ApplyExternalVelocity(new Vector2(dirX * force, up));
            _control?.LockControl(_knockbackLockTime);
        }

        // Called from HER_Die animation event on last frame (optional)
        public void OnDieAnimEnd() { }
    }
}
