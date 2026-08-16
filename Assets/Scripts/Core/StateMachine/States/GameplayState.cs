using System;
using Core.Gameplay.Dungeon;
using Core.StateMachine.Features;
using IncrementalRPG.Scripts.AudioManager;
using Reflex.Attributes;
using UI;
using UnityEngine;

namespace Core.StateMachine.States
{
    public class GameplayState : IGameState
    {
        [Inject] private GameplayFeature _gameplay;
        [Inject] private MenuCanvasView _menuCanvas;
        [Inject] private HudView _hudView;
        [Inject] private SessionEndPopupView _sessionEndPopup;
        [Inject] private PauseMenuController _pauseMenu;
        [Inject] private AudioManager _audioManager;
        [Inject] private DemoEndPopupProvider _demoEndPopupProvider;

        public event Action OnGoToHubRequested;
        public event Action OnMainMenuRequested;

        private DemoEndPopupView _demoEndPopup;

        public void Enter()
        {
            _demoEndPopup = _demoEndPopupProvider?.View;
            _menuCanvas.gameObject.SetActive(false);
            _hudView.gameObject.SetActive(true);
            if (_pauseMenu != null)
                _pauseMenu.OnPauseChanged += HandlePauseChanged;
            _pauseMenu?.EnableForGameplay();
            _gameplay.OnSessionExpired += HandleSessionExpired;
            _gameplay.OnLevelTransitionStarted += HandleLevelTransitionStarted;
            _hudView.OnLevelTransitionOpeningStarted += HandleLevelTransitionOpeningStarted;
            _hudView.OnLevelTransitionLampAnimationStarted += HandleLevelTransitionLampAnimationStarted;
            _gameplay.OnDemoLimitReached += HandleDemoLimitReached;
            _gameplay.Enable();
        }

        public void Exit(GameStateExitReason reason)
        {
            _gameplay.OnSessionExpired -= HandleSessionExpired;
            _gameplay.OnLevelTransitionStarted -= HandleLevelTransitionStarted;
            _hudView.OnLevelTransitionOpeningStarted -= HandleLevelTransitionOpeningStarted;
            _hudView.OnLevelTransitionLampAnimationStarted -= HandleLevelTransitionLampAnimationStarted;
            _gameplay.OnDemoLimitReached -= HandleDemoLimitReached;
            if (_pauseMenu != null)
                _pauseMenu.OnPauseChanged -= HandlePauseChanged;
            _gameplay.SetPaused(false);
            _pauseMenu?.DisableForGameplay();
            _audioManager?.StopLavaLoop();
            _gameplay.Disable();
            _sessionEndPopup.Hide();
            _demoEndPopup?.Hide();
            _hudView.gameObject.SetActive(false);
            _menuCanvas.gameObject.SetActive(reason == GameStateExitReason.StateChange);
        }

        public void Tick(float deltaTime) => _gameplay.Tick(deltaTime);

        private void HandlePauseChanged(bool isPaused)
        {
            _gameplay.SetPaused(isPaused);
        }

        private void HandleSessionExpired()
        {
            _pauseMenu?.DisableForGameplay();

            var recordResult = _gameplay.SessionRecordResult;
            _audioManager?.PlaySessionEnd();
            _sessionEndPopup.Show(_gameplay.SessionGold, _gameplay.SessionKills,
                recordResult.IsNewGoldRecord, recordResult.IsNewKillsRecord, () =>
            {
                OnGoToHubRequested?.Invoke();
            });
        }

        private void HandleLevelTransitionStarted(DungeonLevelConfig nextLevel, int nextLevelIndex,
            float closeDuration, float holdDuration, float openDuration)
        {
            _audioManager?.PlayCurtainClose();
        }

        private void HandleLevelTransitionOpeningStarted()
        {
            _audioManager?.PlayCurtainOpen();
        }

        private void HandleLevelTransitionLampAnimationStarted()
        {
            _audioManager?.PlayLevelCounterOn();
        }

        private void HandleDemoLimitReached(DungeonConfig dungeon, DungeonLevelConfig level, int levelIndex)
        {
            _pauseMenu?.DisableForGameplay();

            if (_demoEndPopup == null)
            {
                Debug.LogError("[GameplayState] Demo limit was reached, but DemoEndPopupView is not assigned. Continuing gameplay to avoid a soft lock.");
                HandleDemoContinueClicked();
                return;
            }

            _demoEndPopup.Show(HandleDemoContinueClicked, HandleDemoMainMenuClicked);
        }

        private void HandleDemoContinueClicked()
        {
            _demoEndPopup?.Hide();
            _gameplay.ContinueAfterDemoLimitReached();
            _pauseMenu?.EnableForGameplay();
        }

        private void HandleDemoMainMenuClicked()
        {
            _demoEndPopup?.Hide();
            OnMainMenuRequested?.Invoke();
        }
    }
}
