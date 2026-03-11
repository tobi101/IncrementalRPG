namespace SlotAdventure.Scripts.Reflex
{
    public interface IAwakeable
    {
        void OnAwake();
    }

    public interface IStartable
    {
        void OnStart();
    }

    public interface ITickable
    {
        void Tick(float deltaTime);
    }

    public interface IFixedTickable
    {
        void FixedTick(float fixedDeltaTime);
    }

    public interface ILateTickable
    {
        void LateTick(float deltaTime);
    }
}