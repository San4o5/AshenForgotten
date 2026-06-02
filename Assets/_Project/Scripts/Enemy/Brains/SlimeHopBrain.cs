using AshenForgotten.Combat;
using UnityEngine;

namespace AshenForgotten.Enemies
{
    public sealed class SlimeHopBrain : IEnemyBrain
    {
        private readonly float _hopHorizontalSpeed;
        private readonly float _hopVerticalImpulse;
        private readonly float _minIdleTime;
        private readonly float _maxIdleTime;
        private readonly float _patrolRange;
        private readonly float _detectionRange;
        private readonly float _loseTargetRange;
        private readonly float _groundCheckOffsetY;

        private EnemyContext _ctx;
        private Vector2 _origin;
        private float _facingDir = 1f;
        private float _idleTimer;

        private enum State { WaitOnGround, Hop, Airborne }
        private State _state = State.WaitOnGround;

        private enum Mode { Patrol, Chase }
        private Mode _mode = Mode.Patrol;

        public SlimeHopBrain(
            float hopHorizontalSpeed, float hopVerticalImpulse,
            float minIdleTime, float maxIdleTime,
            float patrolRange, float detectionRange, float loseTargetRange,
            float groundCheckOffsetY = 0.5f)
        {
            _hopHorizontalSpeed = hopHorizontalSpeed;
            _hopVerticalImpulse = hopVerticalImpulse;
            _minIdleTime = minIdleTime;
            _maxIdleTime = maxIdleTime;
            _patrolRange = patrolRange;
            _detectionRange = detectionRange;
            _loseTargetRange = loseTargetRange;
            _groundCheckOffsetY = groundCheckOffsetY;
        }

        public void Init(EnemyContext ctx)
        {
            _ctx = ctx;
            _origin = ctx.Self.position;
            _facingDir = ctx.View.FacingRight ? 1f : -1f;
            ResetIdleTimer();
        }

        public void Tick(float dt)
        {
            UpdateMode();

            switch (_state)
            {
                case State.WaitOnGround: TickWait(dt); break;
                case State.Hop:          TickHop();    break;
                case State.Airborne:     TickAir();    break;
            }
        }

        public void OnDamaged(in HitInfo hit)
        {
            // Aggro immediately on hit
            _mode = Mode.Chase;
            // Yield to EnemyHealth's knockback velocity — switch to Airborne so TickWait won't zero it next frame
            _state = State.Airborne;
        }

        private void UpdateMode()
        {
            if (_ctx.Player == null) { _mode = Mode.Patrol; return; }
            float dist = Vector2.Distance(_ctx.Self.position, _ctx.Player.position);
            if (_mode == Mode.Patrol && dist <= _detectionRange) _mode = Mode.Chase;
            else if (_mode == Mode.Chase && dist >= _loseTargetRange) _mode = Mode.Patrol;
        }

        private void TickWait(float dt)
        {
            // Damp horizontal drift while waiting (still affected by gravity)
            _ctx.Body.linearVelocity = new Vector2(0f, _ctx.Body.linearVelocity.y);

            _idleTimer -= dt;
            if (_idleTimer > 0f) return;

            _state = State.Hop;
        }

        private void TickHop()
        {
            float dirX = ChooseHopDirection();
            // dirX == 0 means "no safe ground either side" — hop straight up in place.
            if (dirX != 0f && Mathf.Sign(_facingDir) != Mathf.Sign(dirX)) Flip(dirX);

            _ctx.Body.linearVelocity = new Vector2(dirX * _hopHorizontalSpeed, _hopVerticalImpulse);
            _state = State.Airborne;
        }

        private void TickAir()
        {
            // Wait until we're falling AND touching ground again
            if (_ctx.Body.linearVelocity.y > 0.05f) return;

            if (IsGrounded())
            {
                // Snap horizontal to zero so slime doesn't slide
                _ctx.Body.linearVelocity = new Vector2(0f, _ctx.Body.linearVelocity.y);
                _state = State.WaitOnGround;
                ResetIdleTimer();
            }
        }

        private float ChooseHopDirection()
        {
            float dir;

            if (_mode == Mode.Chase && _ctx.Player != null)
            {
                float d = Mathf.Sign(_ctx.Player.position.x - _ctx.Self.position.x);
                dir = d == 0f ? _facingDir : d;
            }
            else
            {
                // Patrol: reverse at bounds, otherwise keep facing
                float dxFromOrigin = _ctx.Self.position.x - _origin.x;
                if (dxFromOrigin > _patrolRange) dir = -1f;
                else if (dxFromOrigin < -_patrolRange) dir = 1f;
                else dir = _facingDir;
            }

            // Edge guard: never hop off a ledge into a pit. If there's no ground
            // ahead in the chosen direction, turn around instead (applies to both
            // patrol and chase so slimes won't suicide into gaps).
            if (!HasGroundAhead(dir))
            {
                dir = -dir;
                // If there's no ground the other way either, stay put this hop.
                if (!HasGroundAhead(dir)) return 0f;
            }

            return dir;
        }

        private bool HasGroundAhead(float dir)
        {
            // Probe ahead at roughly the slime's hop landing distance and cast down
            // to detect a ledge/pit. Scaled by hop speed so faster slimes look further.
            float aheadDistance = Mathf.Max(1.5f, _hopHorizontalSpeed * 0.7f);
            const float probeDepth = 1.5f;
            Vector2 origin = (Vector2)_ctx.Self.position
                             + Vector2.right * (dir * aheadDistance)
                             + Vector2.down * _groundCheckOffsetY;
            var hit = Physics2D.Raycast(origin, Vector2.down, probeDepth, _ctx.GroundLayer);
            return hit.collider != null;
        }

        private bool IsGrounded()
        {
            // Cast a short ray straight down from slime's pivot
            Vector2 origin = (Vector2)_ctx.Self.position + Vector2.down * _groundCheckOffsetY;
            var hit = Physics2D.Raycast(origin, Vector2.down, 0.15f, _ctx.GroundLayer);
            return hit.collider != null;
        }

        private void Flip(float dir)
        {
            _facingDir = dir;
            _ctx.View.SetFacing(dir);
        }

        private void ResetIdleTimer()
        {
            _idleTimer = Random.Range(_minIdleTime, _maxIdleTime);
        }
    }
}
