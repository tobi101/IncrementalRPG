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

        private readonly SaveStorage _saveStorage = new SaveStorage();

        private void Awake()
        {
            if (_view == null)
                _view = GetComponent<MainMenuView>();
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
            Bind(MainMenuAction.NewGame, StartNewGame);
            Bind(MainMenuAction.Continue, ContinueGame);
            Bind(MainMenuAction.Settings, ShowSettings);
            Bind(MainMenuAction.Authors, ShowAuthors);
            Bind(MainMenuAction.Exit, ExitGame);
        }

        private void Unsubscribe()
        {
            Unbind(MainMenuAction.NewGame, StartNewGame);
            Unbind(MainMenuAction.Continue, ContinueGame);
            Unbind(MainMenuAction.Settings, ShowSettings);
            Unbind(MainMenuAction.Authors, ShowAuthors);
            Unbind(MainMenuAction.Exit, ExitGame);
        }

        private void Bind(MainMenuAction action, UnityEngine.Events.UnityAction handler)
        {
            var buttonView = _view.GetButton(action);
            if (buttonView == null || buttonView.Button == null)
                return;

            buttonView.Button.onClick.RemoveListener(handler);
            buttonView.Button.onClick.AddListener(handler);
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

        private void RefreshContinueButton()
        {
            _view.SetContinueVisible(_saveStorage.HasSave());
        }

        private void StartNewGame()
        {
            if (_deleteSaveOnNewGame)
                _saveStorage.Delete();

            LoadGameScene();
        }

        private void ContinueGame()
        {
            if (!_saveStorage.HasSave())
            {
                RefreshContinueButton();
                return;
            }

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
