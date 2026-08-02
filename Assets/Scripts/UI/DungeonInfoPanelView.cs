using System;
using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Entity;
using TMPro;
using UI.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DungeonInfoPanelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _dungeonNameText;
        [SerializeField] private TMP_Text _levelNumberText;
        [SerializeField] private GameObject[] _playableContent;
        [SerializeField] private GameObject[] _unavailableContent;
        [SerializeField] private Transform _enemyIconsContainer;
        [SerializeField] private Image _enemyIconPrefab;
        [SerializeField] private TooltipView _enemyTooltipView;
        [SerializeField, Min(1)] private int _maxEnemyIcons = 6;
        [SerializeField] private Button _startButton;

        private readonly List<Image> _spawnedEnemyIcons = new();
        private Action _onStartClicked;
        private LocalizedStringBinding _dungeonNameBinding;
        private GridLayoutGroup _enemyGridLayout;
        private Vector2 _enemyGridDefaultCellSize;

        private void Awake()
        {
            _dungeonNameBinding = new LocalizedStringBinding(_dungeonNameText);
            UIButtonAudio.EnsureOn(_startButton);
            InitializeEnemyGridLayout();

            if (_enemyIconPrefab != null)
                _enemyIconPrefab.gameObject.SetActive(false);

            if (_startButton != null)
                _startButton.onClick.AddListener(HandleStartClicked);
        }

        public void Bind(DungeonConfig dungeon, int levelIndex, Action onStartClicked)
        {
            ClearEnemyIcons();
            _onStartClicked = onStartClicked;

            DungeonLevelConfig level = null;
            var hasPlayableLevel = dungeon != null
                                   && levelIndex >= 0
                                   && dungeon.TryGetLevel(levelIndex, out level)
                                   && level != null
                                   && level.IsPlayable;

            if (!hasPlayableLevel)
            {
                ShowUnavailable();
                return;
            }

            SetUnavailableVisible(false);
            SetPlayableContentVisible(true);

            if (_dungeonNameText != null)
                _dungeonNameBinding.Bind(dungeon.displayName);

            if (_levelNumberText != null)
            {
                _levelNumberText.gameObject.SetActive(true);
                _levelNumberText.text = (levelIndex + 1).ToString();
            }

            BuildEnemyIcons(level);

            if (_startButton != null)
            {
                _startButton.gameObject.SetActive(true);
                _startButton.interactable = true;
            }
        }

        public void BindUnavailable()
        {
            ClearEnemyIcons();
            _onStartClicked = null;
            ShowUnavailable();
        }

        private void ShowUnavailable()
        {
            _onStartClicked = null;
            _dungeonNameBinding?.Clear();

            SetPlayableContentVisible(false);
            SetUnavailableVisible(true);

            if (_startButton != null)
            {
                _startButton.interactable = false;
                _startButton.gameObject.SetActive(false);
            }
        }

        private void SetUnavailableVisible(bool visible)
        {
            if (_unavailableContent == null)
                return;

            foreach (var content in _unavailableContent)
            {
                if (content != null && content.activeSelf != visible)
                    content.SetActive(visible);
            }
        }

        private void SetPlayableContentVisible(bool visible)
        {
            if (_playableContent != null)
            {
                foreach (var content in _playableContent)
                {
                    if (content != null && content.activeSelf != visible)
                        content.SetActive(visible);
                }
            }

            if (_enemyIconsContainer != null && _enemyIconsContainer.gameObject.activeSelf != visible)
                _enemyIconsContainer.gameObject.SetActive(visible);
        }

        private void BuildEnemyIcons(DungeonLevelConfig level)
        {
            if (_enemyIconsContainer == null || _enemyIconPrefab == null
                || level.spawnTable == null || level.spawnTable.entries == null)
                return;

            var addedConfigs = new HashSet<EntityConfig>();

            foreach (var entry in level.spawnTable.entries)
            {
                if (_spawnedEnemyIcons.Count >= _maxEnemyIcons)
                    break;

                var config = entry != null ? entry.config : null;
                if (config == null || config.featureType != FeatureType.None || config.icon == null)
                    continue;

                if (!addedConfigs.Add(config))
                    continue;

                var icon = Instantiate(_enemyIconPrefab, _enemyIconsContainer);
                icon.gameObject.SetActive(true);
                icon.sprite = config.icon;
                icon.enabled = true;

                var tooltipTrigger = icon.GetComponent<EnemyIconTooltipTrigger>();
                if (tooltipTrigger == null)
                    tooltipTrigger = icon.gameObject.AddComponent<EnemyIconTooltipTrigger>();

                tooltipTrigger.Bind(config, _enemyTooltipView);
                _spawnedEnemyIcons.Add(icon);
            }

            FitEnemyIconGrid();
        }

        private void ClearEnemyIcons()
        {
            if (_enemyTooltipView != null)
                _enemyTooltipView.Hide();

            foreach (var icon in _spawnedEnemyIcons)
            {
                if (icon != null)
                    Destroy(icon.gameObject);
            }

            _spawnedEnemyIcons.Clear();
            ResetEnemyIconGrid();
        }

        private void InitializeEnemyGridLayout()
        {
            if (_enemyGridLayout != null || _enemyIconsContainer == null)
                return;

            _enemyGridLayout = _enemyIconsContainer.GetComponent<GridLayoutGroup>();
            if (_enemyGridLayout != null)
                _enemyGridDefaultCellSize = _enemyGridLayout.cellSize;
        }

        private void FitEnemyIconGrid()
        {
            InitializeEnemyGridLayout();
            ResetEnemyIconGrid();

            if (_enemyGridLayout == null || _spawnedEnemyIcons.Count == 0
                || _enemyGridLayout.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
                return;

            var columns = Mathf.Max(1, _enemyGridLayout.constraintCount);
            var rows = Mathf.CeilToInt((float)_spawnedEnemyIcons.Count / columns);
            if (rows <= 1)
                return;

            if (_enemyIconsContainer is not RectTransform containerRect)
                return;

            Canvas.ForceUpdateCanvases();
            var availableWidth = containerRect.rect.width
                                 - _enemyGridLayout.padding.horizontal
                                 - _enemyGridLayout.spacing.x * (columns - 1);
            var availableHeight = containerRect.rect.height
                                  - _enemyGridLayout.padding.vertical
                                  - _enemyGridLayout.spacing.y * (rows - 1);

            if (availableWidth <= 0f || availableHeight <= 0f
                || _enemyGridDefaultCellSize.x <= 0f || _enemyGridDefaultCellSize.y <= 0f)
                return;

            var scale = Mathf.Min(1f,
                availableWidth / (columns * _enemyGridDefaultCellSize.x),
                availableHeight / (rows * _enemyGridDefaultCellSize.y));
            _enemyGridLayout.cellSize = _enemyGridDefaultCellSize * scale;
        }

        private void ResetEnemyIconGrid()
        {
            InitializeEnemyGridLayout();

            if (_enemyGridLayout != null && _enemyGridDefaultCellSize != Vector2.zero)
                _enemyGridLayout.cellSize = _enemyGridDefaultCellSize;
        }

        private void HandleStartClicked()
        {
            _onStartClicked?.Invoke();
        }

        private void OnDestroy()
        {
            if (_startButton != null)
                _startButton.onClick.RemoveListener(HandleStartClicked);

            _dungeonNameBinding?.Dispose();
            ClearEnemyIcons();
        }
    }
}
