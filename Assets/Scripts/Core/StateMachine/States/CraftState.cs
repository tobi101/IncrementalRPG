using Core.StateMachine.Features;
using Reflex.Attributes;

namespace Core.StateMachine.States
{
    public class CraftState : IGameState
    {
        [Inject] private ForgeFeature _forge;

        public void Enter() => _forge.Enable();
        public void Exit(GameStateExitReason reason) => _forge.Disable();
        public void Tick(float deltaTime) { }
    }
}
