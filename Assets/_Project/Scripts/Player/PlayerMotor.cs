using UnityEngine;

namespace AshenForgotten.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlayerMotor : MonoBehaviour, IPlayerMotor
    {
        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 4f;
        [SerializeField] private float _runMultiplier = 1.8f;
        [SerializeField] private float _airSpeedMultiplier = 1.15f;

        [Header("Jump")]
        [SerializeField] private float _jumpVelocity = 13f;
        [SerializeField] private float _coyoteTime = 0.1f;
        [SerializeField] private float _jumpBufferTime = 0.1f;
        [SerializeField] private float _jumpCutMultiplier = 0.5f;

        [Header("Gravity")]
        [SerializeField] private float _gravity = 60f;
        [SerializeField] private float _maxFallSpeed = 25f;

        [Header("Collision")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _skinWidth = 0.02f;

        private Rigidbody2D _rb;
        private CapsuleCollider2D _capsule;
        private IPlayerInput _input;

        private Vector2 _velocity;
        private bool _isGrounded;
        private bool _facingRight = true;

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _isJumping;     // true between launch and apex/peak; gates jump-cut
        private bool _jumpHeldPrev;

        public Vector2 Velocity   => _velocity;
        public bool IsGrounded    => _isGrounded;
        public bool FacingRight   => _facingRight;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _capsule = GetComponent<CapsuleCollider2D>();
            _input = GetComponent<IPlayerInput>();

            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void FixedUpdate()
        {
            if (_input == null) return;
            float dt = Time.fixedDeltaTime;

            UpdateTimers(dt);

            // Horizontal velocity from input — small air-control bonus for longer jumps
            float targetSpeed = _walkSpeed * (_input.RunHeld ? _runMultiplier : 1f);
            if (!_isGrounded) targetSpeed *= _airSpeedMultiplier;
            _velocity.x = _input.Horizontal * targetSpeed;

            // Jump: buffered/coyote press OR auto-repeat while Space is held on ground
            bool wantsJump = (_jumpBufferTimer > 0f) || _input.JumpHeld;
            if (wantsJump && _coyoteTimer > 0f)
            {
                _velocity.y = _jumpVelocity;
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
                _isGrounded = false;
                _isJumping = true;
            }

            // Variable jump height — cut upward velocity ONCE on Space release,
            // and only while we're still ascending from THIS jump (not from a bounce/knockback).
            bool jumpReleased = _jumpHeldPrev && !_input.JumpHeld;
            if (_isJumping && jumpReleased && _velocity.y > 0f)
            {
                _velocity.y *= _jumpCutMultiplier;
                _isJumping = false;   // consume — never cut twice in one jump
            }
            // End the jump state once we reach apex/start falling, so a future cut can't apply mid-fall
            if (_isJumping && _velocity.y <= 0f) _isJumping = false;

            _jumpHeldPrev = _input.JumpHeld;

            // Gravity
            _velocity.y -= _gravity * dt;
            if (_velocity.y < -_maxFallSpeed) _velocity.y = -_maxFallSpeed;

            // Resolve motion with sweep tests
            Vector2 delta = _velocity * dt;
            delta = MoveX(delta);
            delta = MoveY(delta);

            _rb.MovePosition(_rb.position + delta);

            // Facing
            if (_input.Horizontal > 0.01f && !_facingRight) Flip();
            else if (_input.Horizontal < -0.01f && _facingRight) Flip();
        }

        private void UpdateTimers(float dt)
        {
            if (_isGrounded) _coyoteTimer = _coyoteTime;
            else if (_coyoteTimer > 0f) _coyoteTimer -= dt;

            if (_input.JumpPressed) _jumpBufferTimer = _jumpBufferTime;
            else if (_jumpBufferTimer > 0f) _jumpBufferTimer -= dt;
        }

        private Vector2 MoveX(Vector2 delta)
        {
            if (Mathf.Approximately(delta.x, 0f)) return delta;

            float dir = Mathf.Sign(delta.x);
            float dist = Mathf.Abs(delta.x) + _skinWidth;
            var hit = CapsuleCast(new Vector2(dir, 0f), dist);

            if (hit.collider != null)
            {
                delta.x = (hit.distance - _skinWidth) * dir;
                _velocity.x = 0f;
            }
            return delta;
        }

        private Vector2 MoveY(Vector2 delta)
        {
            // Always cast a tiny bit downward to check ground even when stationary
            float dir = delta.y >= 0f ? 1f : -1f;
            float dist = Mathf.Abs(delta.y) + _skinWidth;
            var hit = CapsuleCast(new Vector2(0f, dir), dist);

            if (hit.collider != null)
            {
                delta.y = (hit.distance - _skinWidth) * dir;
                if (dir < 0f) _isGrounded = true;     // landed
                _velocity.y = 0f;
            }
            else
            {
                if (dir < 0f) _isGrounded = false;    // airborne
            }
            return delta;
        }

        private RaycastHit2D CapsuleCast(Vector2 direction, float distance)
        {
            Vector2 origin = (Vector2)_capsule.bounds.center;
            Vector2 size = _capsule.size * (Vector2)transform.lossyScale;
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);

            return Physics2D.CapsuleCast(
                origin,
                size,
                _capsule.direction,
                0f,
                direction,
                distance,
                _groundLayer
            );
        }

        public void ApplyExternalVelocity(Vector2 v)
        {
            _velocity = v;
        }

        public void FreezeForDeath()
        {
            _velocity = Vector2.zero;
            enabled = false;
        }

        public void Flip()
        {
            _facingRight = !_facingRight;
            var s = transform.localScale;
            s.x = -s.x;
            transform.localScale = s;
        }
    }
}
