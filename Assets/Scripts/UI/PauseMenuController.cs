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
        [SerializeField] private SideMenuFlyoutView[] _sideMenus;
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
        private SideMenuFlyoutView _settingsReturnSideMenu;

        public bool IsOpen => Root.activeSelf;
        public bool IsGameplayPaused => _isGameplayPaused;

        public void RegisterSideMenu(SideMenuFlyoutView sideMenu)
        {
            var count = _sideMenus.Length;
            Array.Resize(ref _sideMenus, count + 1);
            _sideMenus[count] = sideMenu;

            if (isActiveAndEnabled)
                SubscribeSideMenu(sideMenu);
        }

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
            CloseAllSideMenus();
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
            _settingsReturnSideMenu = null;
            CloseAllSideMenus();
            Root.SetActive(true);
            SetPausePanelVisible(true);
            _settingsMenu?.Close();
            SetGameplayInputBlocked(true);
            SetGameplayPaused(true);
        }

        public void Close()
        {
            _settingsReturnSideMenu = null;
            CloseAllSideMenus();
            _settingsMenu?.Close();
            SetPausePanelVisible(false);
            Root.SetActive(false);
            SetGameplayInputBlocked(false);
            SetGameplayPaused(false);
        }

        private void OpenSettings()
        {
            _settingsReturnSideMenu = null;
            CloseAllSideMenus();
            Root.SetActive(true);
            SetPausePanelVisible(false);
            _settingsMenu?.Open();
            SetGameplayInputBlocked(true);
            SetGameplayPaused(true);
        }

        private void OpenSettingsFromSideMenu(SideMenuFlyoutView source)
        {
            _settingsReturnSideMenu = source;
            CloseAllSideMenus();
            Root.SetActive(true);
            SetPausePanelVisible(false);
            _settingsMenu?.Open();
            SetGameplayInputBlocked(false);
            SetGameplayPaused(false);
        }

        private void ReturnToPause()
        {
            _settingsReturnSideMenu = null;
            Root.SetActive(true);
            _settingsMenu?.Close();
            SetPausePanelVisible(true);
            SetGameplayInputBlocked(true);
            SetGameplayPaused(true);
        }

        private void ReturnFromSettings()
        {
            if (_isGameplayInputBlockingAllowed)
            {
                ReturnToPause();
                return;
            }

            var returnSideMenu = _settingsReturnSideMenu;
            _settingsReturnSideMenu = null;
            _settingsMenu?.Close();
            SetPausePanelVisible(false);
            Root.SetActive(false);
            SetGameplayInputBlocked(false);
            SetGameplayPaused(false);
            returnSideMenu?.Open();
        }

        private void ExitToMainMenu()
        {
            if (_isExitingToMainMenu)
                return;

            _isExitingToMainMenu = true;

            // Session gold is applied only by GameplayFeature, while already collected shards live
            // directly in Player. Saving here preserves shards without committing unfinished session gold.
            _saveService?.Save();

            Close();
            _stateMachine?.ExitCurrent(GameStateExitReason.SceneUnload);
            SceneManager.LoadSceneAsync(_mainMenuSceneName);
        }

        private void ExitToMainMenu(SideMenuFlyoutView source)
        {
            ExitToMainMenu();
        }

        private void ExitApplication(SideMenuFlyoutView source)
        {
            _saveService?.Save();
            Close();
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void HandlePausePerformed(InputAction.CallbackContext context)
        {
            if (!_isPauseShortcutEnabled)
                return;

            if (IsSettingsOpen)
            {
                ReturnFromSettings();
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
                AddListener(_settingsMenu.BackButton, ReturnFromSettings);

            SubscribeSideMenus();
            AddListeners(_exitToMainMenuButtons, ExitToMainMenu);
        }

        private void UnsubscribeButtons()
        {
            RemoveListener(_resumeButton, Close);
            RemoveListener(_settingsButton, OpenSettings);

            if (_settingsMenu != null && _settingsMenu.BackButton != null)
                RemoveListener(_settingsMenu.BackButton, ReturnFromSettings);

            UnsubscribeSideMenus();
            RemoveListeners(_exitToMainMenuButtons, ExitToMainMenu);
        }

        private void SubscribeSideMenus()
        {
            if (_sideMenus == null)
                return;

            foreach (var sideMenu in _sideMenus)
            {
                if (sideMenu == null)
                    continue;

                SubscribeSideMenu(sideMenu);
            }
        }

        private void SubscribeSideMenu(SideMenuFlyoutView sideMenu)
        {
            sideMenu.SettingsRequested -= OpenSettingsFromSideMenu;
            sideMenu.SettingsRequested += OpenSettingsFromSideMenu;
            sideMenu.MainMenuRequested -= ExitToMainMenu;
            sideMenu.MainMenuRequested += ExitToMainMenu;
            sideMenu.ExitRequested -= ExitApplication;
            sideMenu.ExitRequested += ExitApplication;
        }

        private void UnsubscribeSideMenus()
        {
            if (_sideMenus == null)
                return;

            foreach (var sideMenu in _sideMenus)
            {
                if (sideMenu == null)
                    continue;

                sideMenu.SettingsRequested -= OpenSettingsFromSideMenu;
                sideMenu.MainMenuRequested -= ExitToMainMenu;
                sideMenu.ExitRequested -= ExitApplication;
            }
        }

        private void CloseAllSideMenus()
        {
            if (_sideMenus == null)
                return;

            foreach (var sideMenu in _sideMenus)
                sideMenu?.CloseImmediate();
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
        }
    }
}
