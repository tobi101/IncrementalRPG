using Reflex.Attributes;
using UI;

namespace Core.StateMachine.Features
{
    public class HubFeature : IGameFeature
    {
        [Inject] private HubView _view;

        public void Initialize() => _view.gameObject.SetActive(false);

        public void Enable() => _view.gameObject.SetActive(true);

        public void Disable() { }

        public void Tick(float deltaTime) { }
    }
}
