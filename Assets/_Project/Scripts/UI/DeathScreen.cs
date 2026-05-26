using System.Collections;
using AshenForgotten.Combat;
using AshenForgotten.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AshenForgotten.UI
{
    public class DeathScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private float _showDelay = 1.5f;
        [SerializeField] private string _menuSceneName = "MainMenu";

        private IHealth _health;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_restartButton != null) _restartButton.onClick.AddListener(OnRestartClicked);
            if (_menuButton != null) _menuButton.onClick.AddListener(OnMenuClicked);
        }

        private void Start()
        {
            TryBind();
        }

        private void OnEnable()
        {
            TryBind();
        }

        private void OnDisable()
        {
            if (_health is Health h) h.Died -= OnPlayerDied;
        }

        private void OnDestroy()
        {
            if (_restartButton != null) _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_menuButton != null) _menuButton.onClick.RemoveListener(OnMenuClicked);
        }

        private void TryBind()
        {
            if (_health != null) return;
            _health = PlayerService.PlayerHealth;
            if (_health is Health h) h.Died += OnPlayerDied;
        }

        private void OnPlayerDied()
        {
            StartCoroutine(ShowAfterDelay());
        }

        private IEnumerator ShowAfterDelay()
        {
            yield return new WaitForSeconds(_showDelay);
            if (_panel != null) _panel.SetActive(true);
        }

        public void OnRestartClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnMenuClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_menuSceneName);
        }
    }
}
