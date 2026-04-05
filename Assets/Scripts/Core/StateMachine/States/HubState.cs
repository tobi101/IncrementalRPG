using Core.StateMachine.Features;
using Reflex.Attributes;

namespace Core.StateMachine.States
{
    public class HubState : IGameState
    {
        [Inject] private HubFeature _hub;

        public void Enter() => _hub.Enable();

        public void Exit() => _hub.Disable();

        public void Tick(float deltaTime) { }
    }
}
