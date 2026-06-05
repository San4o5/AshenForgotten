using UnityEngine;

namespace AshenForgotten.Player
{
    // Loads persistent wallet state on scene start. Attach to Player or a dedicated bootstrap GameObject.
    public class PlayerWalletBootstrap : MonoBehaviour
    {
        // When true, the wallet is reset to 0 on every scene start/restart instead of loading
        // the persisted value. Useful for video capture / fresh-run demos.
        [SerializeField] private bool _resetOnStart = true;

        private void Awake()
        {
            if (_resetOnStart)
                PlayerWallet.ResetWallet();
            else
                PlayerWallet.Load();
        }
    }
}
