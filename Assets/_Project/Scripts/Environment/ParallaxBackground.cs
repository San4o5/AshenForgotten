using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    
    // How much this layer moves relative to camera (0 = static, 1 = moves with camera)
    [SerializeField] private float parallaxSpeed = 0.5f;
    
    // Enable infinite scrolling for this layer
    [SerializeField] private bool infiniteHorizontal = true;

    private float _textureUnitSizeX;
    private Vector3 _lastCameraPosition;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _lastCameraPosition = mainCamera.transform.position;
        
        // Get the width of the sprite for infinite scrolling calculation
        _textureUnitSizeX = _spriteRenderer.bounds.size.x;
    }

    private void LateUpdate()
    {
        // Calculate how much camera moved since last frame
        Vector3 deltaMovement = mainCamera.transform.position - _lastCameraPosition;
        
        // Move this layer by a fraction of camera movement (creates parallax effect)
        transform.position += new Vector3(deltaMovement.x * parallaxSpeed, 0f, 0f);
        _lastCameraPosition = mainCamera.transform.position;

        // Reposition sprite when camera moves too far (creates infinite scroll)
        if (infiniteHorizontal)
        {
            float cameraX = mainCamera.transform.position.x;
            if (Mathf.Abs(cameraX - transform.position.x) >= _textureUnitSizeX)
            {
                float offset = (cameraX - transform.position.x) % _textureUnitSizeX;
                transform.position = new Vector3(
                    cameraX + offset, 
                    transform.position.y, 
                    transform.position.z
                );
            }
        }
    }
}