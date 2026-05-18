using UnityEngine;
using UnityEngine.InputSystem;

namespace AshenForgotten.Player
{
    public class PlayerInput : MonoBehaviour, IPlayerInput
    {
        public float Horizontal { get; private set; }
        public bool RunHeld { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool AttackPressed { get; private set; }

        private bool _enabled = true;

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!enabled) Clear();
        }

        private void Clear()
        {
            Horizontal = 0f;
            RunHeld = false;
            JumpPressed = false;
            JumpHeld = false;
            AttackPressed = false;
        }

        private void Update()
        {
            if (!_enabled) { Clear(); return; }

            var kb = Keyboard.current;
            if (kb == null) { Clear(); return; }

            Horizontal = 0f;
            if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) Horizontal = -1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) Horizontal = 1f;

            RunHeld       = kb.leftShiftKey.isPressed && Horizontal != 0f;
            JumpPressed   = kb.spaceKey.wasPressedThisFrame;
            JumpHeld      = kb.spaceKey.isPressed;
            AttackPressed = kb.xKey.wasPressedThisFrame;
        }
    }
}
