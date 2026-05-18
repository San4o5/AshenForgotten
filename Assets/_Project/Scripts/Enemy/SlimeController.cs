using UnityEngine;

namespace AshenForgotten.Enemies
{
    public class SlimeController : EnemyController
    {
        [Header("Hop")]
        [SerializeField] private float _hopHorizontalSpeed = 3f;
        [SerializeField] private float _hopVerticalImpulse = 7f;

        [Header("Idle Between Hops")]
        [SerializeField] private float _minIdleTime = 0.6f;
        [SerializeField] private float _maxIdleTime = 1.4f;

        [Header("Patrol / Detection")]
        [SerializeField] private float _patrolRange = 5f;
        [SerializeField] private float _detectionRange = 7f;
        [SerializeField] private float _loseTargetRange = 12f;

        [Header("Ground Check")]
        [SerializeField] private float _groundCheckOffsetY = 0.5f;

        protected override IEnemyBrain CreateBrain()
        {
            return new SlimeHopBrain(
                _hopHorizontalSpeed, _hopVerticalImpulse,
                _minIdleTime, _maxIdleTime,
                _patrolRange, _detectionRange, _loseTargetRange,
                _groundCheckOffsetY
            );
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _loseTargetRange);

            // Patrol bounds (drawn relative to current position, not actual origin — Brain owns origin at runtime)
            Gizmos.color = Color.cyan;
            Vector3 left  = transform.position + Vector3.left  * _patrolRange;
            Vector3 right = transform.position + Vector3.right * _patrolRange;
            Gizmos.DrawLine(left + Vector3.down * 0.2f, left + Vector3.up * 0.2f);
            Gizmos.DrawLine(right + Vector3.down * 0.2f, right + Vector3.up * 0.2f);

            // Ground check ray
            Gizmos.color = Color.green;
            Vector3 gOrigin = transform.position + Vector3.down * _groundCheckOffsetY;
            Gizmos.DrawLine(gOrigin, gOrigin + Vector3.down * 0.15f);
        }
    }
}
