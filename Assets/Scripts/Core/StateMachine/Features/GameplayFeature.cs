using System;
using System.Collections.Generic;
using System.Linq;
using Core.Gameplay;
using UnityEngine;
using Core.Gameplay.Dungeon;
using Core.TestSkillTree;
using Entity;
using IncrementalRPG.Scripts.AudioManager;
using IncrementalRPG.Scripts.Core;
using Model;
using Reflex.Attributes;
using Utils;

namespace Core.StateMachine.Features
{
    public class GameplayFeature : IGameFeature
    {
        [Inject] private IEnumerable<IService> _servicesEnumerable;
        [Inject] private DungeonList _dungeonList;
        [Inject] private DungeonSelectionService _dungeonSelection;
        [Inject] private IsometricGradientTilemapGenerator _generator;
        [Inject] private SpawnService _spawnService;
        [Inject] private TileGrid _tileGrid;
        [Inject] private DamageZone _damageZone;
        [Inject] private Player _player;
        [Inject] private SkillTreeService _skillTree;
        [Inject] private AudioManager _audioManager;

        public event Action OnSessionExpired;
        public event Action<BigDouble, int> OnSessionGoldEarned;
        public event Action<int> OnSessionKillsChanged;
        public event Action<int, int> OnLevelKillGoalChanged;
        public event Action<DungeonConfig, DungeonLevelConfig, int> OnDungeonLevelChanged;
        public event Action<DungeonLevelConfig, int, float> OnLevelTransitionStarted;
        public event Action<DungeonLevelConfig, int> OnLevelTransitionFinished;

        public BigDouble SessionGold => _sessionGold;
        public int SessionKills => _sessionKills;
        public int LevelKills => _levelKills;
        public int CurrentLevelKillGoal => _currentLevel != null ? _currentLevel.killGoal : 0;
        public int CurrentLevelIndex => _currentLevelIndex;
        public DungeonConfig CurrentDungeon => _currentDungeon;
        public DungeonLevelConfig CurrentLevel => _currentLevel;
        public SessionRecordResult SessionRecordResult => _sessionRecordResult;

        private enum RunState
        {
            Inactive,
            Ready,
            Playing,
            Transitioning,
            Expired
        }

        private List<IService> _services;
        private DungeonConfig _currentDungeon;
        private DungeonLevelConfig _currentLevel;
        private int _currentLevelIndex;
        private int _levelKills;
        private int _pendingLevelTransitionIndex = -1;
        private int _transitionTargetLevelIndex = -1;
        private float _transitionTimer;
        private float _sessionTimeLeft;
        private float _sessionTotalTime;
        private RunState _runState = RunState.Inactive;
        private BigDouble _sessionGold;
        private int _sessionKills;
        private SessionRecordResult _sessionRecordResult;

        public void Initialize()
        {
            _services = _servicesEnumerable.ToList();

            foreach (var service in _services)
                service.Initialize();

            _spawnService.OnCreatureKilled += HandleCreatureKilled;
        }

        public void Enable()
        {
            _sessionGold = BigDouble.Zero;
            _sessionKills = 0;
            _levelKills = 0;
            _sessionRecordResult = default;
            _pendingLevelTransitionIndex = -1;
            _transitionTargetLevelIndex = -1;
            _transitionTimer = 0f;
            _currentDungeon = _dungeonSelection.GetSelectedOrDefault(_dungeonList);

            if (_currentDungeon == null || !_currentDungeon.HasPlayableLevels)
            {
                _runState = RunState.Inactive;
                Debug.LogError("[GameplayFeature] Cannot start gameplay because no playable dungeon is selected.");
                return;
            }

            var startLevelIndex = _dungeonSelection.GetStartLevelIndex(_currentDungeon);
            if (!ApplyLevel(startLevelIndex))
            {
                _runState = RunState.Inactive;
                return;
            }

            SpawnInitialEntities();
            _runState = RunState.Ready;
        }

        public void StartSession()
        {
            if (_runState == RunState.Ready)
                _runState = RunState.Playing;
        }

        public void Disable()
        {
            _runState = RunState.Inactive;
            _pendingLevelTransitionIndex = -1;
            _transitionTargetLevelIndex = -1;
            _transitionTimer = 0f;
            _spawnService.DespawnAll();
        }

        public void Tick(float deltaTime)
        {
            switch (_runState)
            {
                case RunState.Ready:
                    TickReady();
                    break;
                case RunState.Playing:
                    TickPlaying(deltaTime);
                    break;
                case RunState.Transitioning:
                    TickTransition(deltaTime);
                    break;
            }
        }

        private void TickReady()
        {
            _damageZone.UpdateAim();
        }

        private void TickPlaying(float deltaTime)
        {
            _sessionTimeLeft -= deltaTime;
            var progress = _sessionTotalTime > 0f
                ? 1f - Mathf.Clamp01(_sessionTimeLeft / _sessionTotalTime)
                : 1f;
            _generator.CameraAutoFitter.AnimateLava(progress);
            _audioManager?.SetLavaLoopProgress(progress);

            if (_sessionTimeLeft <= 0f)
            {
                ExpireSession();
                return;
            }

            foreach (var service in _services)
                service.Update(deltaTime);

            if (_pendingLevelTransitionIndex >= 0 && _runState == RunState.Playing)
                BeginLevelTransition(_pendingLevelTransitionIndex);
        }

        private void TickTransition(float deltaTime)
        {
            _transitionTimer -= deltaTime;

            if (_transitionTimer <= 0f)
                CompleteLevelTransition();
        }

        private void ExpireSession()
        {
            _runState = RunState.Expired;
            _sessionRecordResult = _player.UpdateSessionRecords(_sessionGold, _sessionKills);
            OnSessionExpired?.Invoke();
        }

        private void BeginLevelTransition(int nextLevelIndex)
        {
            if (!_currentDungeon.TryGetLevel(nextLevelIndex, out var nextLevel) || nextLevel == null || !nextLevel.IsPlayable)
            {
                Debug.LogWarning($"[GameplayFeature] Cannot transition to dungeon level index {nextLevelIndex}.");
                _pendingLevelTransitionIndex = -1;
                return;
            }

            _runState = RunState.Transitioning;
            _pendingLevelTransitionIndex = -1;
            _transitionTargetLevelIndex = nextLevelIndex;
            _transitionTimer = Mathf.Max(0f, nextLevel.transitionDuration);

            _spawnService.DespawnAll();
            OnLevelTransitionStarted?.Invoke(nextLevel, nextLevelIndex, _transitionTimer);

            if (_transitionTimer <= 0f)
                CompleteLevelTransition();
        }

        private void CompleteLevelTransition()
        {
            var targetIndex = _transitionTargetLevelIndex;
            _transitionTargetLevelIndex = -1;
            _transitionTimer = 0f;

            if (!ApplyLevel(targetIndex))
            {
                _runState = RunState.Inactive;
                return;
            }

            SpawnInitialEntities();
            _runState = RunState.Playing;
            _dungeonSelection.MarkLevelReached(_currentDungeon, _currentLevelIndex);
            OnLevelTransitionFinished?.Invoke(_currentLevel, _currentLevelIndex);
        }

        private void HandleCreatureKilled(Vector2Int coord, int amount)
        {
            var levelGoldMultiplier = _currentLevel != null ? _currentLevel.goldDropMultiplier : 1f;
            var finalAmount = Mathf.RoundToInt(amount * levelGoldMultiplier * _skillTree.GetMultiplier(StatType.GoldDrop));

            if (finalAmount > 0)
            {
                _player.GoldTotal += finalAmount;
                _sessionGold += finalAmount;
                OnSessionGoldEarned?.Invoke(_sessionGold, finalAmount);
            }

            _levelKills++;
            _sessionKills++;
            OnSessionKillsChanged?.Invoke(_sessionKills);
            OnLevelKillGoalChanged?.Invoke(_levelKills, CurrentLevelKillGoal);

            if (_runState != RunState.Playing) return;
            if (_pendingLevelTransitionIndex >= 0) return;
            if (_currentLevel == null || _currentLevel.killGoal <= 0) return;
            if (_levelKills < _currentLevel.killGoal) return;

            if (TryGetNextPlayableLevel(out var nextLevelIndex))
                _pendingLevelTransitionIndex = nextLevelIndex;
        }

        private void SpawnInitialEntities()
        {
            var totalTiles = _tileGrid.TotalTileCount;
            var enemyDensity = Mathf.Max(0f, _currentLevel.initialEnemySpawnDensity
                                             + _skillTree.GetBonus(StatType.InitialEnemySpawnDensity));
            var bombDensity = Mathf.Max(0f, _currentLevel.initialBombSpawnDensity
                                            + _skillTree.GetBonus(StatType.InitialBombSpawnDensity));

            var enemyCount = Mathf.RoundToInt(totalTiles * enemyDensity);
            var bombCount = Mathf.RoundToInt(totalTiles * bombDensity);

            _spawnService.SpawnInitial(enemyCount);
            _spawnService.SpawnInitial(bombCount, FeatureType.Bomb);
        }

        private bool ApplyLevel(int levelIndex)
        {
            if (_currentDungeon == null || !_currentDungeon.TryGetLevel(levelIndex, out var level) || level == null)
            {
                Debug.LogError($"[GameplayFeature] Dungeon level index {levelIndex} is missing.");
                return false;
            }

            if (!level.IsPlayable)
            {
                Debug.LogError($"[GameplayFeature] Dungeon level '{level.name}' is not playable. Assign spawn table and tilemap generation config.");
                return false;
            }

            _currentLevelIndex = levelIndex;
            _currentLevel = level;
            _levelKills = 0;

            ConfigureSpawn(level);
            GenerateLevelMap(level);
            ResetLevelTimer(level);
            _generator.CameraAutoFitter.PrepareLavaAnimation();

            OnDungeonLevelChanged?.Invoke(_currentDungeon, _currentLevel, _currentLevelIndex);
            OnLevelKillGoalChanged?.Invoke(_levelKills, CurrentLevelKillGoal);

            return true;
        }

        private void ConfigureSpawn(DungeonLevelConfig level)
        {
            _spawnService.SetLevel(level);

            var spawnInterval = level.spawnInterval * (1f - _skillTree.GetBonus(StatType.SpawnSpeed));
            _spawnService.SetSpawnInterval(Mathf.Max(spawnInterval, level.minSpawnInterval));

            if (level.featureSpawnConfigs == null) return;

            foreach (var fc in level.featureSpawnConfigs)
            {
                var featureInterval = fc.spawnInterval * (1f - _skillTree.GetBonus(fc.spawnSpeedStat));
                _spawnService.SetFeatureSpawnInterval(fc.featureType, Mathf.Max(featureInterval, fc.minSpawnInterval));
            }
        }

        private void GenerateLevelMap(DungeonLevelConfig level)
        {
            _generator.config = level.tilemapGenerationConfig;
            _generator.Size = level.minPlayZoneSize + (int)_skillTree.GetBonus(StatType.MapSize);
            _generator.Generate();
            _tileGrid.Initialize(_generator.TargetTilemap);
        }

        private void ResetLevelTimer(DungeonLevelConfig level)
        {
            _sessionTotalTime = (100f / Mathf.Max(0.0001f, level.heatIndex))
                              - (_player.ArmorIndex / 2.5f)
                              + _skillTree.GetBonus(StatType.SessionTime);
            _sessionTimeLeft = _sessionTotalTime;
        }

        private bool TryGetNextPlayableLevel(out int nextLevelIndex)
        {
            nextLevelIndex = _currentLevelIndex + 1;
            return _currentDungeon != null
                   && _currentDungeon.TryGetLevel(nextLevelIndex, out var nextLevel)
                   && nextLevel != null
                   && nextLevel.IsPlayable;
        }
    }
}
