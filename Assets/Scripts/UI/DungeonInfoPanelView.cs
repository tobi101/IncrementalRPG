using System;
using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Entity;
using TMPro;
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

        private void Awake()
        {
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
                _dungeonNameText.text = dungeon.DisplayName;

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
        }

        private void HandleStartClicked()
        {
            _onStartClicked?.Invoke();
        }

        private void OnDestroy()
        {
            if (_startButton != null)
                _startButton.onClick.RemoveListener(HandleStartClicked);

            ClearEnemyIcons();
        }
    }
}
