using AshenForgotten.Combat;
using AshenForgotten.Player;
using UnityEngine;

namespace AshenForgotten.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyHealth))]
    public abstract class EnemyController : MonoBehaviour, IEnemyView, IBrainHitNotifier
    {
        [Header("References")]
        [SerializeField] protected Transform _edgeCheck;
        [SerializeField] protected LayerMask _groundLayer;

        protected Rigidbody2D _rb;
        protected Animator _animator;
        protected EnemyHealth _health;
        protected IEnemyBrain _brain;
        protected EnemyContext _ctx;

        private float _facingDir = 1f;
        private static readonly int HashSpeed = Animator.StringToHash("Speed");

        public bool FacingRight => _facingDir >= 0f;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _health = GetComponent<EnemyHealth>();
        }

        protected virtual void Start()
        {
            _ctx = new EnemyContext
            {
                Self = transform,
                Body = _rb,
                Animator = _animator,
                Player = PlayerService.PlayerTransform,
                EdgeCheck = _edgeCheck,
                GroundLayer = _groundLayer,
                View = this,
            };
            _brain = CreateBrain();
            _brain.Init(_ctx);

            _health.HealthChanged += OnHealthChanged;
        }

        protected virtual void OnDestroy()
        {
            if (_health != null) _health.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            // Hook for HP bars / VFX later
        }

        protected virtual void FixedUpdate()
        {
            if (_health.IsDead)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                SetSpeed(0f);
                return;
            }

            // Refresh player ref if it appeared after Start (or respawn)
            if (_ctx.Player == null && PlayerService.IsAvailable)
                _ctx.Player = PlayerService.PlayerTransform;

            _brain?.Tick(Time.fixedDeltaTime);
            SetSpeed(Mathf.Abs(_rb.linearVelocity.x));
        }

        protected abstract IEnemyBrain CreateBrain();

        // --- IEnemyView ---
        public void SetFacing(float dir)
        {
            if (Mathf.Approximately(dir, 0f)) return;
            float newDir = dir > 0f ? 1f : -1f;
            if (Mathf.Approximately(_facingDir, newDir)) return;
            _facingDir = newDir;
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * newDir;
            transform.localScale = s;
        }

        public void SetSpeed(float speed)
        {
            if (_animator != null) _animator.SetFloat(HashSpeed, speed);
        }

        public void NotifyBrainOfHit(in HitInfo hit)
        {
            _brain?.OnDamaged(in hit);
        }
    }
}
