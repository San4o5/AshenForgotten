using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class AshServantController : MonoBehaviour
{
    private enum State { Patrol, Chase }

    [Header("Patrol")]
    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private float _patrolRange = 3f;

    [Header("Chase")]
    [SerializeField] private float _detectionRange = 6f;
    [SerializeField] private float _chaseSpeed = 3f;
    [SerializeField] private float _loseTargetRange = 9f;

    [Header("Ground Check")]
    [SerializeField] private Transform _frontEdgeCheck;
    [SerializeField] private float _edgeCheckDistance = 1f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Target")]
    [SerializeField] private string _playerTag = "Player";

    private Rigidbody2D _rb;
    private Animator _animator;
    private EnemyHealth _health;
    private Transform _player;

    private State _state = State.Patrol;
    private float _facingDir = 1f;
    private Vector2 _patrolOrigin;

    private static readonly int HashSpeed = Animator.StringToHash("Speed");

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<EnemyHealth>();
        _patrolOrigin = transform.position;
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag(_playerTag);
        if (p != null) _player = p.transform;
    }

    private void FixedUpdate()
    {
        if (_health.IsDead) { _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y); UpdateAnim(0f); return; }

        UpdateState();
        switch (_state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Chase:  TickChase();  break;
        }

        UpdateAnim(Mathf.Abs(_rb.linearVelocity.x));
    }

    private void UpdateState()
    {
        if (_player == null) return;
        float dist = Vector2.Distance(transform.position, _player.position);
        if (_state == State.Patrol && dist <= _detectionRange) _state = State.Chase;
        else if (_state == State.Chase && dist >= _loseTargetRange) _state = State.Patrol;
    }

    private void TickPatrol()
    {
        // Reverse at patrol range bounds
        float dxFromOrigin = transform.position.x - _patrolOrigin.x;
        if (dxFromOrigin > _patrolRange && _facingDir > 0f) Flip(-1f);
        else if (dxFromOrigin < -_patrolRange && _facingDir < 0f) Flip(1f);

        // Reverse at edge (no ground in front)
        if (_frontEdgeCheck != null)
        {
            bool groundAhead = Physics2D.OverlapCircle(_frontEdgeCheck.position, 0.15f, _groundLayer);
            if (!groundAhead) Flip(-_facingDir);
        }

        _rb.linearVelocity = new Vector2(_facingDir * _patrolSpeed, _rb.linearVelocity.y);
    }

    private void TickChase()
    {
        if (_player == null) { _state = State.Patrol; return; }
        float dirX = Mathf.Sign(_player.position.x - transform.position.x);
        if (dirX != 0f && Mathf.Sign(_facingDir) != dirX) Flip(dirX);

        // Stop short on edge
        if (_frontEdgeCheck != null)
        {
            bool groundAhead = Physics2D.OverlapCircle(_frontEdgeCheck.position, 0.15f, _groundLayer);
            if (!groundAhead) { _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y); return; }
        }

        _rb.linearVelocity = new Vector2(dirX * _chaseSpeed, _rb.linearVelocity.y);
    }

    private void Flip(float newDir)
    {
        _facingDir = newDir;
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (newDir >= 0f ? 1f : -1f);
        transform.localScale = s;
    }

    private void UpdateAnim(float speed)
    {
        if (_animator != null) _animator.SetFloat(HashSpeed, speed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _loseTargetRange);
        if (_frontEdgeCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_frontEdgeCheck.position, 0.15f);
        }
    }
}
