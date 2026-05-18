namespace AshenForgotten.Player
{
    public interface IPlayerControl
    {
        void SetControlEnabled(bool enabled);
        void LockControl(float duration);
    }
}
