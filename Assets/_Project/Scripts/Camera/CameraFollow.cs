using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private float offsetY = 1.5f;

    [Header("Dead Zone")]
    [SerializeField] private float deadZoneWidth = 3f;
    [SerializeField] private float deadZoneHeight = 2f;

    [Header("Follow")]
    [SerializeField] private float smoothTime = 0.12f;
    [SerializeField] private float maxSpeed = 20f;

    private Vector3 _desiredPosition;
    private Vector3 _velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + new Vector3(0f, offsetY, 0f);

        float halfW = deadZoneWidth * 0.5f;
        float halfH = deadZoneHeight * 0.5f;

        // Move desired position only when target exits dead zone
        if (targetPos.x > _desiredPosition.x + halfW)
            _desiredPosition.x = targetPos.x - halfW;
        else if (targetPos.x < _desiredPosition.x - halfW)
            _desiredPosition.x = targetPos.x + halfW;

        if (targetPos.y > _desiredPosition.y + halfH)
            _desiredPosition.y = targetPos.y - halfH;
        else if (targetPos.y < _desiredPosition.y - halfH)
            _desiredPosition.y = targetPos.y + halfH;

        Vector3 desired = new Vector3(_desiredPosition.x, _desiredPosition.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime, maxSpeed);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireCube(
            new Vector3(_desiredPosition.x, _desiredPosition.y, 0f),
            new Vector3(deadZoneWidth, deadZoneHeight, 0f)
        );
    }
}
