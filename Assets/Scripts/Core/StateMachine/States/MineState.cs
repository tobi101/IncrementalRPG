using Core.StateMachine.Features;
using Reflex.Attributes;

namespace Core.StateMachine.States
{
    public class MineState : IGameState
    {
        [Inject] private HubFeature _hub;

        public void Enter() => _hub.Enable();
        public void Exit(GameStateExitReason reason) { }
        public void Tick(float deltaTime) { }
    }
}
