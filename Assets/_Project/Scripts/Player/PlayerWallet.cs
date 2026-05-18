using System;
using UnityEngine;

namespace AshenForgotten.Player
{
    public static class PlayerWallet
    {
        private const string KeyCoins = "Wallet.Coins";

        public static int Coins { get; private set; }
        public static event Action<int> CoinsChanged;

        private static bool _loaded;

        public static void Load()
        {
            Coins = PlayerPrefs.GetInt(KeyCoins, 0);
            _loaded = true;
            CoinsChanged?.Invoke(Coins);
        }

        public static void AddCoins(int amount)
        {
            if (amount <= 0) return;
            if (!_loaded) Load();
            Coins += amount;
            Save();
            CoinsChanged?.Invoke(Coins);
        }

        public static void ResetWallet()
        {
            Coins = 0;
            Save();
            CoinsChanged?.Invoke(Coins);
        }

        private static void Save()
        {
            PlayerPrefs.SetInt(KeyCoins, Coins);
            PlayerPrefs.Save();
        }
    }
}
