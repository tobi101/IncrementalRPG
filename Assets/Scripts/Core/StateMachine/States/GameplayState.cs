using System;
using Core.StateMachine.Features;
using Reflex.Attributes;
using UI;

namespace Core.StateMachine.States
{
    public class GameplayState : IGameState
    {
        [Inject] private GameplayFeature _gameplay;
        [Inject] private MenuCanvasView _menuCanvas;
        [Inject] private HudView _hudView;
        [Inject] private SessionEndPopupView _sessionEndPopup;

        public event Action OnGoToHubRequested;

        public void Enter()
        {
            _menuCanvas.gameObject.SetActive(false);
            _hudView.gameObject.SetActive(true);
            _gameplay.Enable();
            _gameplay.OnSessionExpired += HandleSessionExpired;
        }

        public void Exit()
        {
            _gameplay.OnSessionExpired -= HandleSessionExpired;
            _gameplay.Disable();
            _sessionEndPopup.Hide();
            _hudView.gameObject.SetActive(false);
            _menuCanvas.gameObject.SetActive(true);
        }

        public void Tick(float deltaTime) => _gameplay.Tick(deltaTime);

        private void HandleSessionExpired()
        {
            var recordResult = _gameplay.SessionRecordResult;
            _sessionEndPopup.Show(_gameplay.SessionGold, _gameplay.SessionKills,
                recordResult.IsNewGoldRecord, recordResult.IsNewKillsRecord, () =>
            {
                OnGoToHubRequested?.Invoke();
            });
        }
    }
}
