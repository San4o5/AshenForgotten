using UnityEngine;

namespace AshenForgotten.Player
{
    public class PlayerFacade : MonoBehaviour, IPlayerControl
    {
        private PlayerInput _input;
        private PlayerMotor _motor;
        private PlayerCombat _combat;
        private PlayerHealth _health;
        private Animator _animator;

        private bool _controlEnabled = true;
        private float _controlLockTimer;

        private static readonly int HashSpeed            = Animator.StringToHash("Speed");
        private static readonly int HashIsRunning        = Animator.StringToHash("IsRunning");
        private static readonly int HashIsGrounded       = Animator.StringToHash("IsGrounded");
        private static readonly int HashVerticalVelocity = Animator.StringToHash("VerticalVelocity");

        private void Awake()
        {
            _input    = GetComponent<PlayerInput>();
            _motor    = GetComponent<PlayerMotor>();
            _combat   = GetComponent<PlayerCombat>();
            _health   = GetComponent<PlayerHealth>();
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            PlayerService.Register(transform, _health, _health);
        }

        private void OnDisable()
        {
            PlayerService.Unregister(transform);
        }

        private void Update()
        {
            if (_controlLockTimer > 0f)
            {
                _controlLockTimer -= Time.deltaTime;
                if (_controlLockTimer <= 0f) ApplyControlState();
            }
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;
            _animator.SetFloat(HashSpeed, Mathf.Abs(_input != null ? _input.Horizontal : 0f));
            _animator.SetBool(HashIsRunning, _input != null && _input.RunHeld);
            _animator.SetBool(HashIsGrounded, _motor != null && _motor.IsGrounded);
            _animator.SetFloat(HashVerticalVelocity, _motor != null ? _motor.Velocity.y : 0f);
        }

        public void SetControlEnabled(bool enabled)
        {
            _controlEnabled = enabled;
            ApplyControlState();
        }

        public void LockControl(float duration)
        {
            _controlLockTimer = Mathf.Max(_controlLockTimer, duration);
            ApplyControlState();
        }

        private void ApplyControlState()
        {
            bool effective = _controlEnabled && _controlLockTimer <= 0f;
            if (_input != null) _input.SetEnabled(effective);
            if (_combat != null) _combat.SetEnabled(effective);
        }
    }
}
