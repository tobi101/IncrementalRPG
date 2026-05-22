using Core.StateMachine.Features;
using IncrementalRPG.Scripts.AudioManager;
using Reflex.Attributes;
using UI;

namespace Core.StateMachine.States
{
    public class HubState : IGameState
    {
        [Inject] private HubFeature _hub;
        [Inject] private HudView _hudView;
        [Inject] private AudioManager _audioManager;

        public void Enter()
        {
            _audioManager?.PlayMusic(MusicTrack.Hub);
            _hudView.gameObject.SetActive(false);
            _hub.Enable();
        }

        public void Exit() => _hub.Disable();

        public void Tick(float deltaTime) { }
    }
}
