using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _runMultiplier = 1.8f;
    [SerializeField] private float _jumpForce = 13f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Attack")]
    [SerializeField] private float _attackCooldown = 0.3f;
    [SerializeField] private AttackHitbox _attackHitbox;

    private Rigidbody2D _rb;
    private Animator _animator;

    private float _horizontalInput;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _facingRight = true;

    private bool _jumpPressed;
    private float _attackTimer;
    private bool _controlEnabled = true;
    private float _controlLockTimer;

    private static readonly int HashSpeed            = Animator.StringToHash("Speed");
    private static readonly int HashIsRunning        = Animator.StringToHash("IsRunning");
    private static readonly int HashIsGrounded       = Animator.StringToHash("IsGrounded");
    private static readonly int HashVerticalVelocity = Animator.StringToHash("VerticalVelocity");
    private static readonly int HashAttack           = Animator.StringToHash("Attack");

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_controlLockTimer > 0f) _controlLockTimer -= Time.deltaTime;
        if (!CanAct()) { ClearInput(); UpdateAnimator(); return; }

        GatherInput();
        HandleAttack();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        _isGrounded = Physics2D.OverlapCircle(
            _groundCheck.position,
            _groundCheckRadius,
            _groundLayer
        );

        if (!CanAct()) return;

        HandleMovement();
        HandleJump();
    }

    private bool CanAct() => _controlEnabled && _controlLockTimer <= 0f;

    private void ClearInput()
    {
        _horizontalInput = 0f;
        _isRunning = false;
        _jumpPressed = false;
    }

    public void SetControlEnabled(bool enabled)
    {
        _controlEnabled = enabled;
        if (!enabled) ClearInput();
    }

    public void LockControl(float duration)
    {
        _controlLockTimer = Mathf.Max(_controlLockTimer, duration);
    }

    private void GatherInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        _horizontalInput = 0f;
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)  _horizontalInput = -1f;
        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) _horizontalInput = 1f;

        _isRunning = keyboard.leftShiftKey.isPressed && _horizontalInput != 0f;

        if (keyboard.spaceKey.wasPressedThisFrame && _isGrounded)
            _jumpPressed = true;

        // Debug: H deals 1 self-damage to test Hurt/Die without enemies
        if (keyboard.hKey.wasPressedThisFrame)
        {
            var health = GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamageFrom(1, transform.position + Vector3.right * (_facingRight ? 1f : -1f));
        }

        _attackTimer -= Time.deltaTime;
    }

    private void HandleMovement()
    {
        float targetSpeed = _walkSpeed * (_isRunning ? _runMultiplier : 1f);
        _rb.linearVelocity = new Vector2(_horizontalInput * targetSpeed, _rb.linearVelocity.y);

        if (_horizontalInput > 0 && !_facingRight) Flip();
        else if (_horizontalInput < 0 && _facingRight) Flip();
    }

    private void HandleJump()
    {
        if (_jumpPressed)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _jumpPressed = false;
        }
    }

    private void HandleAttack()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (!keyboard.xKey.wasPressedThisFrame) return;
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

    private void UpdateAnimator()
    {
        if (_animator == null) return;
        _animator.SetFloat(HashSpeed, Mathf.Abs(_horizontalInput));
        _animator.SetBool(HashIsRunning, _isRunning);
        _animator.SetBool(HashIsGrounded, _isGrounded);
        _animator.SetFloat(HashVerticalVelocity, _rb.linearVelocity.y);
    }

    private void Flip()
    {
        _facingRight = !_facingRight;
        var s = transform.localScale;
        s.x = -s.x;
        transform.localScale = s;
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
    }
}
