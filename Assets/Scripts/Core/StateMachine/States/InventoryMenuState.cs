using Core.StateMachine.Features;
using Reflex.Attributes;

namespace Core.StateMachine.States
{
    public sealed class InventoryMenuState : IGameState
    {
        [Inject] private InventoryFeature _inventory;

        public void Enter() => _inventory.Enable();

        public void Exit(GameStateExitReason reason) => _inventory.Disable();

        public void Tick(float deltaTime) { }
    }
}
