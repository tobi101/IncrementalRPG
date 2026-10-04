using IncrementalRPG.Scripts.AudioManager;
using Reflex.Attributes;
using UI;
using UI.Classes;

namespace Core.StateMachine.Features
{
    public sealed class ClassesFeature : IGameFeature
    {
        [Inject] private ClassesMenuView _view;
        [Inject] private HubFeature _hub;
        [Inject] private MenuBackdropView _backdrop;
        [Inject] private AudioManager _audio;
        public void Initialize()
        {
            _view.Hide();
            _backdrop.Hide();
        }
        public void Enable()
        {
            _hub.Disable();
            _audio?.PlayMusic(MusicTrack.Hub);
            _backdrop.Show();
            _view.Show();
        }
        public void Disable() { _view.Hide(); _backdrop.Hide(); }
        public void Tick(float deltaTime) { }
    }
}
