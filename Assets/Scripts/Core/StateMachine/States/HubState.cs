using Core.StateMachine.Features;
using Reflex.Attributes;
using UI;

namespace Core.StateMachine.States
{
    public class HubState : IGameState
    {
        [Inject] private HubFeature _hub;
        [Inject] private HudView _hudView;

        public void Enter()
        {
            _hudView.gameObject.SetActive(false);
            _hub.Enable();
        }

        public void Exit() => _hub.Disable();

        public void Tick(float deltaTime) { }
    }
}
