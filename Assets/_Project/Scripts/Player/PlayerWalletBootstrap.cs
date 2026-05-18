using UnityEngine;

namespace AshenForgotten.Player
{
    // Loads persistent wallet state on scene start. Attach to Player or a dedicated bootstrap GameObject.
    public class PlayerWalletBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            PlayerWallet.Load();
        }
    }
}
