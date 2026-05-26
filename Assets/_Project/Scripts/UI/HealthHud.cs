using AshenForgotten.Combat;
using AshenForgotten.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AshenForgotten.UI
{
    public class HealthHud : MonoBehaviour
    {
        [SerializeField] private Image[] _hearts;
        [SerializeField] private Color _fullColor = Color.white;
        [SerializeField] private Color _emptyColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        private IHealth _health;

        private void OnEnable()
        {
            TryBind();
        }

        private void Start()
        {
            TryBind();
        }

        private void OnDisable()
        {
            if (_health is Health h) h.HealthChanged -= OnHealthChanged;
        }

        private void TryBind()
        {
            if (_health != null) return;
            _health = PlayerService.PlayerHealth;
            if (_health is Health h)
            {
                h.HealthChanged += OnHealthChanged;
                OnHealthChanged(h.Current, h.Max);
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            for (int i = 0; i < _hearts.Length; i++)
            {
                if (_hearts[i] == null) continue;
                _hearts[i].color = i < current ? _fullColor : _emptyColor;
                _hearts[i].enabled = i < max;
            }
        }
    }
}
