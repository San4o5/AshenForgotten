#if UNITY_EDITOR
using AshenForgotten.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AshenForgotten.Player
{
    public class PlayerDebugTools : MonoBehaviour
    {
        [SerializeField] private int _selfDamage = 1;

        private PlayerHealth _health;
        private PlayerMotor _motor;

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();
            _motor  = GetComponent<PlayerMotor>();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || _health == null) return;

            // H = self-damage to test Hurt/Die without enemies
            if (kb.hKey.wasPressedThisFrame)
            {
                float dirX = _motor != null && _motor.FacingRight ? 1f : -1f;
                Vector2 source = (Vector2)transform.position + new Vector2(dirX, 0f);
                _health.TakeDamage(new HitInfo(_selfDamage, source, Vector2.right * -dirX));
            }
        }
    }
}
#endif
