using UnityEngine;

namespace AshenForgotten.CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Offset")]
        [SerializeField] private float _offsetY = 1.5f;

        [Header("Dead Zone")]
        [SerializeField] private float _deadZoneWidth = 3f;
        [SerializeField] private float _deadZoneHeight = 2f;

        [Header("Follow")]
        [SerializeField] private float _smoothTime = 0.12f;
        [SerializeField] private float _maxSpeed = 20f;

        private Vector3 _desiredPosition;
        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 targetPos = _target.position + new Vector3(0f, _offsetY, 0f);

            float halfW = _deadZoneWidth * 0.5f;
            float halfH = _deadZoneHeight * 0.5f;

            if (targetPos.x > _desiredPosition.x + halfW)
                _desiredPosition.x = targetPos.x - halfW;
            else if (targetPos.x < _desiredPosition.x - halfW)
                _desiredPosition.x = targetPos.x + halfW;

            if (targetPos.y > _desiredPosition.y + halfH)
                _desiredPosition.y = targetPos.y - halfH;
            else if (targetPos.y < _desiredPosition.y - halfH)
                _desiredPosition.y = targetPos.y + halfH;

            Vector3 desired = new Vector3(_desiredPosition.x, _desiredPosition.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime, _maxSpeed);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
            Gizmos.DrawWireCube(
                new Vector3(_desiredPosition.x, _desiredPosition.y, 0f),
                new Vector3(_deadZoneWidth, _deadZoneHeight, 0f)
            );
        }
    }
}
