using Reflex.Attributes;
using UI;
using UI.Forge;
namespace Core.StateMachine.Features
{
    public sealed class ForgeFeature : IGameFeature
    {
        [Inject] private CraftView _view;
        [Inject] private HubFeature _hub;
        [Inject] private MenuBackdropView _backdrop;
        public void Initialize() => _view.Hide();
        public void Enable() { _hub.Disable(); _backdrop.Show(); _view.Show(); }
        public void Disable() { _view.Hide(); _backdrop.Hide(); }
        public void Tick(float deltaTime) { }
    }
}
