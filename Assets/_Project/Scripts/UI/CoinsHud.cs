using AshenForgotten.Player;
using TMPro;
using UnityEngine;

namespace AshenForgotten.UI
{
    public class CoinsHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        private void OnEnable()
        {
            PlayerWallet.CoinsChanged += OnCoinsChanged;
            OnCoinsChanged(PlayerWallet.Coins);
        }

        private void OnDisable()
        {
            PlayerWallet.CoinsChanged -= OnCoinsChanged;
        }

        private void OnCoinsChanged(int coins)
        {
            if (_label != null) _label.text = coins.ToString();
        }
    }
}
