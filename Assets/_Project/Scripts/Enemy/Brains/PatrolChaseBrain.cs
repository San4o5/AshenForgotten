using AshenForgotten.Combat;
using UnityEngine;

namespace AshenForgotten.Enemies
{
    public sealed class PatrolChaseBrain : IEnemyBrain
    {
        private readonly float _patrolSpeed;
        private readonly float _patrolRange;
        private readonly float _detectionRange;
        private readonly float _chaseSpeed;
        private readonly float _loseTargetRange;
        private readonly float _edgeCheckRadius;

        private EnemyContext _ctx;
        private Vector2 _origin;
        private float _facingDir = 1f;

        private enum State { Patrol, Chase }
        private State _state = State.Patrol;

        public PatrolChaseBrain(
            float patrolSpeed, float patrolRange,
            float detectionRange, float chaseSpeed, float loseTargetRange,
            float edgeCheckRadius = 0.15f)
        {
            _patrolSpeed = patrolSpeed;
            _patrolRange = patrolRange;
            _detectionRange = detectionRange;
            _chaseSpeed = chaseSpeed;
            _loseTargetRange = loseTargetRange;
            _edgeCheckRadius = edgeCheckRadius;
        }

        public void Init(EnemyContext ctx)
        {
            _ctx = ctx;
            _origin = ctx.Self.position;
            _facingDir = ctx.View.FacingRight ? 1f : -1f;
        }

        public void Tick(float dt)
        {
            UpdateState();
            switch (_state)
            {
                case State.Patrol: TickPatrol(); break;
                case State.Chase:  TickChase();  break;
            }
        }

        public void OnDamaged(in HitInfo hit)
        {
            // Aggro immediately when hit
            _state = State.Chase;
        }

        private void UpdateState()
        {
            if (_ctx.Player == null) return;
            float dist = Vector2.Distance(_ctx.Self.position, _ctx.Player.position);
            if (_state == State.Patrol && dist <= _detectionRange) _state = State.Chase;
            else if (_state == State.Chase && dist >= _loseTargetRange) _state = State.Patrol;
        }

        private void TickPatrol()
        {
            float dxFromOrigin = _ctx.Self.position.x - _origin.x;
            if (dxFromOrigin > _patrolRange && _facingDir > 0f) Flip(-1f);
            else if (dxFromOrigin < -_patrolRange && _facingDir < 0f) Flip(1f);

            if (!HasGroundAhead()) Flip(-_facingDir);

            _ctx.Body.linearVelocity = new Vector2(_facingDir * _patrolSpeed, _ctx.Body.linearVelocity.y);
        }

        private void TickChase()
        {
            if (_ctx.Player == null) { _state = State.Patrol; return; }

            float dirX = Mathf.Sign(_ctx.Player.position.x - _ctx.Self.position.x);
            if (dirX != 0f && Mathf.Sign(_facingDir) != dirX) Flip(dirX);

            if (!HasGroundAhead())
            {
                _ctx.Body.linearVelocity = new Vector2(0f, _ctx.Body.linearVelocity.y);
                return;
            }

            _ctx.Body.linearVelocity = new Vector2(dirX * _chaseSpeed, _ctx.Body.linearVelocity.y);
        }

        private bool HasGroundAhead()
        {
            if (_ctx.EdgeCheck == null) return true;
            return Physics2D.OverlapCircle(_ctx.EdgeCheck.position, _edgeCheckRadius, _ctx.GroundLayer);
        }

        private void Flip(float dir)
        {
            _facingDir = dir;
            _ctx.View.SetFacing(dir);
        }
    }
}
