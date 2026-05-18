using UnityEngine;

namespace AshenForgotten.Items
{
    public interface ICollectible
    {
        void OnCollect(GameObject collector);
    }
}
