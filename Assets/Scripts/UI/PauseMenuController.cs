using System;
using Core.Gameplay;
using Core.Save;
using Core.StateMachine;
using Core.StateMachine.States;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private SettingsMenuController _settingsMenu;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button[] _openPauseButtons;
        [FormerlySerializedAs("_goToHubButtons")]
        [SerializeField] private Button[] _exitToMainMenuButtons;
        [SerializeField] private string _mainMenuSceneName = "MainMenuScene";

        [Inject] private GameStateMachine _stateMachine;
        [Inject] private GameplayInputBlocker _inputBlocker;
        [Inject] private SaveService _saveService;

        public event Action<bool> OnPauseChanged;

        private InputAction _pauseAction;
        private bool _isGameplayInputBlockingAllowed;
        private bool _isPauseShortcutEnabled;
        private bool _isExitingToMainMenu;
        private bool _isGameplayPaused;

        public bool IsOpen => Root.activeSelf;
        public bool IsGameplayPaused => _isGameplayPaused;

        private GameObject Root => _root != null ? _root : gameObject;

        private bool IsSettingsOpen => _settingsMenu != null && _settingsMenu.IsVisible();

        private void Awake()
        {
            _pauseAction = new InputAction("Pause", InputActionType.Button);
            _pauseAction.AddBinding("<Keyboard>/escape");
            _pauseAction.AddBinding("<Gamepad>/start");

            InstallButtonEffects();
            Close();
        }

        private void OnEnable()
        {
            SubscribeButtons();
            if (_pauseAction != null)
                _pauseAction.performed += HandlePausePerformed;
        }

        private void OnDisable()
        {
            UnsubscribeButtons();
            if (_pauseAction == null)
                return;

            _pauseAction.performed -= HandlePausePerformed;
            _pauseAction.Disable();
        }

        private void OnDestroy()
        {
            _pauseAction?.Dispose();
        }

        public void EnableForGameplay()
        {
            _isGameplayInputBlockingAllowed = true;
            _isPauseShortcutEnabled = true;
            _pauseAction?.Enable();
            Close();
        }

        public void DisableForGameplay()
        {
            _isGameplayInputBlockingAllowed = false;
            _isPauseShortcutEnabled = false;
            Close();
            _pauseAction?.Disable();
        }

        public void Open()
        {
            Root.SetActive(true);
            SetPausePanelVisible(true);
            _settingsMenu?.Close();
            SetGameplayInputBlocked(true);
            SetGameplayPaused(true);
        }

        public void Close()
        {
            _settingsMenu?.Close();
            SetPausePanelVisible(false);
            Root.SetActive(false);
            SetGameplayInputBlocked(false);
            SetGameplayPaused(false);
        }

        private void OpenSettings()
        {
            Root.SetActive(true);
            SetPausePanelVisible(false);
            _settingsMenu?.Open();
            SetGameplayInputBlocked(true);
            SetGameplayPaused(true);
        }

        private void ReturnToPause()
        {
            Root.SetActive(true);
            _settingsMenu?.Close();
            SetPausePanelVisible(true);
            SetGameplayInputBlocked(true);
            SetGameplayPaused(true);
        }

        private void ExitToMainMenu()
        {
            if (_isExitingToMainMenu)
                return;

            _isExitingToMainMenu = true;

            var isExitingGameplaySession = _stateMachine != null && _stateMachine.IsCurrent<GameplayState>();

            if (!isExitingGameplaySession)
                _saveService?.Save();

            Close();
            _stateMachine?.ExitCurrent(GameStateExitReason.SceneUnload);
            SceneManager.LoadSceneAsync(_mainMenuSceneName);
        }

        private void HandlePausePerformed(InputAction.CallbackContext context)
        {
            if (!_isPauseShortcutEnabled)
                return;

            if (IsSettingsOpen)
            {
                ReturnToPause();
                return;
            }

            if (IsOpen)
                Close();
            else
                Open();
        }

        private void SubscribeButtons()
        {
            AddListener(_resumeButton, Close);
            AddListener(_settingsButton, OpenSettings);

            if (_settingsMenu != null && _settingsMenu.BackButton != null)
                AddListener(_settingsMenu.BackButton, ReturnToPause);

            AddListeners(_openPauseButtons, Open);
            AddListeners(_exitToMainMenuButtons, ExitToMainMenu);
        }

        private void UnsubscribeButtons()
        {
            RemoveListener(_resumeButton, Close);
            RemoveListener(_settingsButton, OpenSettings);

            if (_settingsMenu != null && _settingsMenu.BackButton != null)
                RemoveListener(_settingsMenu.BackButton, ReturnToPause);

            RemoveListeners(_openPauseButtons, Open);
            RemoveListeners(_exitToMainMenuButtons, ExitToMainMenu);
        }

        private void SetPausePanelVisible(bool visible)
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(visible);
        }

        private void SetGameplayInputBlocked(bool blocked)
        {
            _inputBlocker?.SetBlocked(_isGameplayInputBlockingAllowed && blocked);
        }

        private void SetGameplayPaused(bool paused)
        {
            var shouldPause = _isGameplayInputBlockingAllowed && paused;
            if (_isGameplayPaused == shouldPause)
                return;

            _isGameplayPaused = shouldPause;
            OnPauseChanged?.Invoke(_isGameplayPaused);
        }

        private static void AddListeners(Button[] buttons, UnityEngine.Events.UnityAction listener)
        {
            if (buttons == null)
                return;

            foreach (var button in buttons)
                AddListener(button, listener);
        }

        private static void RemoveListeners(Button[] buttons, UnityEngine.Events.UnityAction listener)
        {
            if (buttons == null)
                return;

            foreach (var button in buttons)
                RemoveListener(button, listener);
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction listener)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(listener);
            button.onClick.AddListener(listener);
        }

        private static void RemoveListener(Button button, UnityEngine.Events.UnityAction listener)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(listener);
        }

        private void InstallButtonEffects()
        {
            UIButtonAudio.InstallInChildren(Root.transform);
            UIButtonPressScaler.InstallInChildren(Root.transform);
            InstallButtonAudio(_openPauseButtons);
            InstallButtonPressScalers(_openPauseButtons);
        }

        private static void InstallButtonAudio(Button[] buttons)
        {
            if (buttons == null)
                return;

            foreach (var button in buttons)
                UIButtonAudio.EnsureOn(button);
        }

        private static void InstallButtonPressScalers(Button[] buttons)
        {
            if (buttons == null)
                return;

            foreach (var button in buttons)
            {
                if (button != null && button.GetComponent<PauseButtonVisualState>() != null)
                    continue;

                UIButtonPressScaler.EnsureOn(button);
            }
        }
    }
}
