using AshenForgotten.Player;
using UnityEngine;

namespace AshenForgotten.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class Coin : MonoBehaviour, ICollectible
    {
        [SerializeField] private int _value = 1;
        [SerializeField] private string _playerTag = "Player";

        private bool _collected;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag(_playerTag)) return;
            OnCollect(other.gameObject);
        }

        public void OnCollect(GameObject collector)
        {
            if (_collected) return;
            _collected = true;

            PlayerWallet.AddCoins(_value);
#if UNITY_EDITOR
            Debug.Log($"[Coin] Picked up. Total: {PlayerWallet.Coins}", this);
#endif

            Destroy(gameObject);
        }
    }
}
