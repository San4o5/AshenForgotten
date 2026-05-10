using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.3f;

    // Component references
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    // Movement state
    private float _horizontalInput;
    private bool _isGrounded;
    private bool _facingRight = true;

    // Jump state
    private bool _jumpPressed;

    // Dash state
    private bool _isDashing;
    private float _dashTimer;
    private float _dashCooldownTimer;

    // Attack state
    private float _attackTimer;
    private Vector2 _attackDirection;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_isDashing) return;

        GatherInput();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        // Check if player is on the ground
        _isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (_isDashing)
        {
            HandleDash();
            return;
        }

        HandleMovement();
        HandleJump();
    }

    private void GatherInput()
    {
        // Read horizontal input from keyboard (new Input System)
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        _horizontalInput = 0f;
        if (keyboard.leftArrowKey.isPressed) _horizontalInput = -1f;
        if (keyboard.rightArrowKey.isPressed) _horizontalInput = 1f;

        // Jump
        if (keyboard.spaceKey.wasPressedThisFrame && _isGrounded)
            _jumpPressed = true;

        // Dash
        if (keyboard.leftShiftKey.wasPressedThisFrame && _dashCooldownTimer <= 0f)
            StartDash();

        _dashCooldownTimer -= Time.deltaTime;
        _attackTimer -= Time.deltaTime;
    }

    private void HandleMovement()
    {
        _rb.linearVelocity = new Vector2(_horizontalInput * moveSpeed, _rb.linearVelocity.y);

        // Flip sprite based on direction
        if (_horizontalInput > 0 && !_facingRight) Flip();
        else if (_horizontalInput < 0 && _facingRight) Flip();
    }

    private void HandleJump()
    {
        if (_jumpPressed)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _jumpPressed = false;
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;

        // Dash in the direction the player is facing
        float dashDirection = _facingRight ? 1f : -1f;
        _rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        // Disable gravity during dash
        _rb.gravityScale = 0f;
    }

    private void HandleDash()
    {
        _dashTimer -= Time.deltaTime;

        if (_dashTimer <= 0f)
        {
            _isDashing = false;
            _rb.gravityScale = 3f;
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }
    }

    private void HandleAttack()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (!keyboard.xKey.wasPressedThisFrame) return;
        if (_attackTimer > 0f) return;

        _attackTimer = attackCooldown;

        // Determine attack direction based on arrow keys held
        if (keyboard.upArrowKey.isPressed)
            _attackDirection = Vector2.up;
        else if (keyboard.downArrowKey.isPressed && !_isGrounded)
            _attackDirection = Vector2.down;
        else
            _attackDirection = _facingRight ? Vector2.right : Vector2.left;

        // TODO: trigger attack hitbox
        Debug.Log($"Attack: {_attackDirection}");
    }

    private void Flip()
    {
        _facingRight = !_facingRight;
        _spriteRenderer.flipX = !_spriteRenderer.flipX;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}