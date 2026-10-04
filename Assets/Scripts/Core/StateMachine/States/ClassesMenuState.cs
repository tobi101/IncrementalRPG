using Core.StateMachine.Features;
using Reflex.Attributes;

namespace Core.StateMachine.States
{
    public sealed class ClassesMenuState : IGameState
    {
        [Inject] private ClassesFeature _classes;
        public void Enter() => _classes.Enable();
        public void Exit(GameStateExitReason reason) => _classes.Disable();
        public void Tick(float deltaTime) { }
    }
}
