namespace AshenForgotten.Player
{
    public interface IPlayerInput
    {
        float Horizontal { get; }
        bool RunHeld { get; }
        bool JumpPressed { get; }
        bool JumpHeld { get; }
        bool AttackPressed { get; }
    }
}
