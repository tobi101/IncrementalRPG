namespace Core.StateMachine
{
    public interface IGameFeature
    {
        void Initialize();
        void Enable();
        void Disable();
        void Tick(float deltaTime);
    }
}
