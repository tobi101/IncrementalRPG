namespace Core.StateMachine
{
    public enum GameStateExitReason
    {
        StateChange,
        SceneUnload
    }

    public interface IGameState
    {
        void Enter();
        void Exit(GameStateExitReason reason);
        void Tick(float deltaTime);
    }
}
