using System;
using Core.Gameplay.Dungeon;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DungeonMenuView : MonoBehaviour
    {
        [SerializeField] private DungeonMapButtonView[] _dungeonButtons;
        [SerializeField] private DungeonInfoPanelView _infoPanel;
        [SerializeField] private Button _closeButton;

        private DungeonList _dungeonList;
        private DungeonSelectionService _dungeonSelection;
        private Action<DungeonConfig> _onDungeonStart;
        private DungeonMapButtonView _selectedButton;
        private DungeonConfig _selectedDungeon;
        private CanvasGroup _canvasGroup;
        private bool _initialized;
        private bool _isOpening;
        private bool _isStarting;

        private void Awake()
        {
            Initialize();

            if (!_isOpening)
                SetVisible(false);
        }

        public void Show(DungeonList dungeonList, DungeonSelectionService dungeonSelection,
            Action<DungeonConfig> onDungeonStart)
        {
            _dungeonList = dungeonList;
            _dungeonSelection = dungeonSelection;
            _onDungeonStart = onDungeonStart;
            _isStarting = false;
            _isOpening = true;
            SetVisible(true);
            _isOpening = false;

            Initialize();
            SetInputInteractable(true);
            BindButtons();
            SelectInitialDungeon();
        }

        public void Hide()
        {
            _isStarting = false;
            SetInputInteractable(true);
            SetVisible(false);
            _onDungeonStart = null;
        }

        private void Initialize()
        {
            if (_initialized)
                return;

            UIButtonAudio.InstallInChildren(this);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _initialized = true;
        }

        private void BindButtons()
        {
            if (_dungeonButtons == null || _dungeonButtons.Length == 0)
            {
                Debug.LogWarning("[DungeonMenuView] Dungeon buttons are not assigned.");
                return;
            }

            for (var i = 0; i < _dungeonButtons.Length; i++)
            {
                var button = _dungeonButtons[i];
                if (button == null) continue;

                var dungeon = button.Dungeon != null ? button.Dungeon : GetDungeonByIndex(i);
                button.Bind(dungeon, HandleDungeonButtonClicked);
                button.SetSelected(false);
            }
        }

        private DungeonConfig GetDungeonByIndex(int index)
        {
            if (_dungeonList == null || _dungeonList.dungeons == null || index < 0 || index >= _dungeonList.dungeons.Length)
                return null;

            return _dungeonList.dungeons[index];
        }

        private void SelectInitialDungeon()
        {
            if (_dungeonButtons != null)
            {
                foreach (var button in _dungeonButtons)
                {
                    if (button == null) continue;

                    SelectButton(button);
                    return;
                }
            }

            _selectedButton = null;
            _selectedDungeon = null;
            if (_infoPanel != null)
                _infoPanel.Bind(null, -1, null);
        }

        private void HandleDungeonButtonClicked(DungeonMapButtonView button)
        {
            SelectButton(button);
        }

        private void SelectButton(DungeonMapButtonView button)
        {
            if (button == null)
                return;

            if (_selectedButton != null)
                _selectedButton.SetSelected(false);

            _selectedButton = button;
            _selectedDungeon = button.Dungeon;
            _selectedButton.SetSelected(true);

            if (_selectedDungeon == null || !_selectedDungeon.HasPlayableLevels)
            {
                if (_infoPanel != null)
                    _infoPanel.BindUnavailable();

                return;
            }

            var startLevelIndex = _dungeonSelection != null
                ? _dungeonSelection.GetStartLevelIndex(_selectedDungeon)
                : _selectedDungeon.FirstPlayableLevelIndex;

            if (_infoPanel != null)
                _infoPanel.Bind(_selectedDungeon, startLevelIndex, HandleStartClicked);
        }

        private void HandleStartClicked()
        {
            if (_isStarting || _selectedDungeon == null || !_selectedDungeon.HasPlayableLevels)
                return;

            var callback = _onDungeonStart;
            if (callback == null)
                return;

            _isStarting = true;
            SetInputInteractable(false);

            var dungeon = _selectedDungeon;
            callback.Invoke(dungeon);
        }

        private void SetInputInteractable(bool interactable)
        {
            if (_canvasGroup == null)
                return;

            _canvasGroup.interactable = interactable;
        }

        private void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
                gameObject.SetActive(visible);
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Hide);
        }
    }
}
