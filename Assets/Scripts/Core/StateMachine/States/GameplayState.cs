using Core.StateMachine.Features;
using Reflex.Attributes;

namespace Core.StateMachine.States
{
    public class GameplayState : IGameState
    {
        [Inject] private GameplayFeature _gameplay;

        public void Enter() => _gameplay.Enable();

        public void Exit() => _gameplay.Disable();

        public void Tick(float deltaTime) => _gameplay.Tick(deltaTime);
    }
}
