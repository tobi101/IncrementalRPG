using Core.Save;
using IncrementalRPG.Scripts.AudioManager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;
        [SerializeField] private string _gameSceneName = "GameScene";
        [SerializeField] private bool _deleteSaveOnNewGame = true;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private bool _playMusicOnStart = true;
        [SerializeField] private IntroPanelPlayer _introPlayer;

        private readonly SaveStorage _saveStorage = new SaveStorage();
        private bool _isLoadingGame;

        private void Awake()
        {
            if (_view == null)
                _view = GetComponent<MainMenuView>();

            if (_introPlayer == null)
                _introPlayer = GetComponentInChildren<IntroPanelPlayer>(true);
        }

        private void OnEnable()
        {
            if (_view == null)
                return;

            _view.HidePanels();
            RefreshContinueButton();
            Subscribe();
        }

        private void Start()
        {
            if (_playMusicOnStart)
                AudioManager.Resolve(_audioManager)?.PlayMusic(MusicTrack.MainMenu);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_view == null)
                return;

            Bind(MainMenuAction.NewGame, RequestNewGame);
            Bind(MainMenuAction.Continue, ContinueGame);
            Bind(MainMenuAction.Settings, ShowSettings);
            Bind(MainMenuAction.Authors, ShowAuthors);
            Bind(MainMenuAction.Exit, ExitGame);
            Bind(_view.NewGameConfirmButton, ConfirmNewGame);
            Bind(_view.NewGameCancelButton, CancelNewGame);
        }

        private void Unsubscribe()
        {
            if (_view == null)
                return;

            Unbind(MainMenuAction.NewGame, RequestNewGame);
            Unbind(MainMenuAction.Continue, ContinueGame);
            Unbind(MainMenuAction.Settings, ShowSettings);
            Unbind(MainMenuAction.Authors, ShowAuthors);
            Unbind(MainMenuAction.Exit, ExitGame);
            Unbind(_view.NewGameConfirmButton, ConfirmNewGame);
            Unbind(_view.NewGameCancelButton, CancelNewGame);
        }

        private void Bind(MainMenuAction action, UnityEngine.Events.UnityAction handler)
        {
            var buttonView = _view.GetButton(action);
            if (buttonView == null || buttonView.Button == null)
                return;

            buttonView.Button.onClick.RemoveListener(handler);
            buttonView.Button.onClick.AddListener(handler);
        }

        private void Bind(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction handler)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(handler);
            button.onClick.AddListener(handler);
        }

        private void Unbind(MainMenuAction action, UnityEngine.Events.UnityAction handler)
        {
            if (_view == null)
                return;

            var buttonView = _view.GetButton(action);
            if (buttonView == null || buttonView.Button == null)
                return;

            buttonView.Button.onClick.RemoveListener(handler);
        }

        private void Unbind(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction handler)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(handler);
        }

        private void RefreshContinueButton()
        {
            _view.SetContinueVisible(_saveStorage.HasSave());
        }

        private void RequestNewGame()
        {
            if (_isLoadingGame)
                return;

            if (_saveStorage.HasSave())
            {
                _view.ShowAttention();
                return;
            }

            StartNewGame();
        }

        private void ConfirmNewGame()
        {
            if (_isLoadingGame)
                return;

            _view.HideAttention();
            StartNewGame();
        }

        private void CancelNewGame()
        {
            if (_isLoadingGame)
                return;

            _view.HideAttention();
        }

        private void StartNewGame()
        {
            if (_isLoadingGame)
                return;

            _isLoadingGame = true;
            _view.HidePanels();
            _view.SetButtonsInteractable(false);

            if (_deleteSaveOnNewGame)
                _saveStorage.Delete();

            AudioManager.Resolve(_audioManager)?.StopMusic(true);

            if (_introPlayer != null)
            {
                _introPlayer.Play(LoadGameScene);
                return;
            }

            LoadGameScene();
        }

        private void ContinueGame()
        {
            if (_isLoadingGame)
                return;

            if (!_saveStorage.HasSave())
            {
                RefreshContinueButton();
                return;
            }

            _isLoadingGame = true;
            _view.HidePanels();
            _view.SetButtonsInteractable(false);
            LoadGameScene();
        }

        private void ShowSettings() => _view.ShowSettings();

        private void ShowAuthors() => _view.ShowAuthors();

        private void ExitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void LoadGameScene()
        {
            SceneManager.LoadSceneAsync(_gameSceneName);
        }
    }
}
