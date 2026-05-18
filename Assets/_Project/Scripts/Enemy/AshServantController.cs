using UnityEngine;

namespace AshenForgotten.Enemies
{
    public class AshServantController : EnemyController
    {
        [Header("Patrol")]
        [SerializeField] private float _patrolSpeed = 1.5f;
        [SerializeField] private float _patrolRange = 3f;

        [Header("Chase")]
        [SerializeField] private float _detectionRange = 6f;
        [SerializeField] private float _chaseSpeed = 3f;
        [SerializeField] private float _loseTargetRange = 9f;

        [Header("Edge Check")]
        [SerializeField] private float _edgeCheckRadius = 0.15f;

        protected override IEnemyBrain CreateBrain()
        {
            return new PatrolChaseBrain(
                _patrolSpeed, _patrolRange,
                _detectionRange, _chaseSpeed, _loseTargetRange,
                _edgeCheckRadius
            );
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _loseTargetRange);
            if (_edgeCheck != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_edgeCheck.position, _edgeCheckRadius);
            }
        }
    }
}
