using System;
using System.Collections.Generic;
using System.Linq;
using Core.Gameplay;
using Core.Gameplay.Shards;
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
        [Inject] private DungeonLevelTransitionConfig _levelTransitionConfig;
        [Inject] private DungeonSelectionService _dungeonSelection;
        [Inject] private IsometricGradientTilemapGenerator _generator;
        [Inject] private SpawnService _spawnService;
        [Inject] private TileGrid _tileGrid;
        [Inject] private DamageZone _damageZone;
        [Inject] private ShardDropService _shardDropService;
        [Inject] private Player _player;
        [Inject] private SkillTreeService _skillTree;
        [Inject] private AudioManager _audioManager;

        public event Action OnSessionExpired;
        public event Action<BigDouble, BigDouble> OnSessionGoldEarned;
        public event Action<int> OnSessionKillsChanged;
        public event Action<int, int> OnLevelKillGoalChanged;
        public event Action<DungeonConfig, DungeonLevelConfig, int> OnDungeonLevelChanged;
        public event Action<DungeonLevelConfig, int, float, float, float> OnLevelTransitionStarted;
        public event Action<DungeonLevelConfig, int> OnLevelTransitionFinished;
        public event Action<DungeonConfig, DungeonLevelConfig, int> OnDemoLimitReached;

        public BigDouble SessionGold => _sessionGold;
        public int SessionKills => _sessionKills;
        public int LevelKills => _levelKills;
        public int CurrentLevelKillGoal => _currentLevel != null ? _currentLevel.killGoal : 0;
        public int CurrentLevelIndex => _currentLevelIndex;
        public DungeonConfig CurrentDungeon => _currentDungeon;
        public DungeonLevelConfig CurrentLevel => _currentLevel;
        public SessionRecordResult SessionRecordResult => _sessionRecordResult;
        public bool IsPaused => _isPaused;

        private enum RunState
        {
            Inactive,
            Ready,
            Playing,
            LootGrace,
            TransitionClosing,
            TransitionOpening,
            DemoLimitReached,
            Expired
        }

        private List<IService> _services;
        private DungeonConfig _currentDungeon;
        private DungeonLevelConfig _currentLevel;
        private int _currentLevelIndex;
        private int _levelKills;
        private int _pendingLevelTransitionIndex = -1;
        private bool _pendingDemoLimit;
        private bool _lootGraceEndsAtDemoLimit;
        private float _lootGraceTimer;
        private int _transitionTargetLevelIndex = -1;
        private float _transitionTimer;
        private float _transitionHoldDuration;
        private float _transitionOpenDuration;
        private float _sessionTimeLeft;
        private float _sessionTotalTime;
        private RunState _runState = RunState.Inactive;
        private BigDouble _sessionGold;
        private int _sessionKills;
        private SessionRecordResult _sessionRecordResult;
        private bool _sessionResultsApplied;
        private bool _isPaused;

        public void Initialize()
        {
            _services = _servicesEnumerable.ToList();

            foreach (var service in _services)
                service.Initialize();

            _spawnService.OnEnemyKilled += HandleEnemyKilled;
        }

        public void Enable()
        {
            _sessionGold = BigDouble.Zero;
            _sessionKills = 0;
            _levelKills = 0;
            _sessionRecordResult = default;
            _sessionResultsApplied = false;
            _isPaused = false;
            _spawnService.SetPaused(false);
            _pendingLevelTransitionIndex = -1;
            _pendingDemoLimit = false;
            _lootGraceEndsAtDemoLimit = false;
            _lootGraceTimer = 0f;
            _transitionTargetLevelIndex = -1;
            _transitionTimer = 0f;
            _transitionHoldDuration = 0f;
            _transitionOpenDuration = 0f;
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

        public void ContinueAfterDemoLimitReached()
        {
            if (_runState != RunState.DemoLimitReached)
                return;

            _dungeonSelection.MarkDemoEndAcknowledged(_currentDungeon);
            RestartSessionFromCurrentLevel();
        }

        public void SetPaused(bool isPaused)
        {
            if (_runState == RunState.Inactive)
                isPaused = false;

            if (_isPaused == isPaused)
                return;

            _isPaused = isPaused;
            _spawnService.SetPaused(_isPaused);
        }

        public void Disable()
        {
            SetPaused(false);
            _runState = RunState.Inactive;
            _pendingLevelTransitionIndex = -1;
            _pendingDemoLimit = false;
            _lootGraceEndsAtDemoLimit = false;
            _lootGraceTimer = 0f;
            _transitionTargetLevelIndex = -1;
            _transitionTimer = 0f;
            _transitionHoldDuration = 0f;
            _transitionOpenDuration = 0f;
            _spawnService.DespawnAll();
            _shardDropService.DespawnAll();
        }

        public void Tick(float deltaTime)
        {
            if (_isPaused)
                return;

            switch (_runState)
            {
                case RunState.Ready:
                    TickReady();
                    break;
                case RunState.Playing:
                    TickPlaying(deltaTime);
                    break;
                case RunState.LootGrace:
                    TickLootGrace(deltaTime);
                    break;
                case RunState.TransitionClosing:
                    TickTransitionClosing(deltaTime);
                    break;
                case RunState.TransitionOpening:
                    TickTransitionOpening(deltaTime);
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

            if (_runState != RunState.Playing)
                return;

            if (_pendingLevelTransitionIndex >= 0)
                BeginLootGrace(_pendingLevelTransitionIndex, false);
            else if (_pendingDemoLimit)
                BeginLootGrace(-1, true);
        }

        private void TickLootGrace(float deltaTime)
        {
            _damageZone.UpdateAim();
            _shardDropService.Update(deltaTime);
            _lootGraceTimer -= deltaTime;

            if (_lootGraceTimer > 0f)
                return;

            if (_lootGraceEndsAtDemoLimit)
            {
                ReachDemoLimit();
                return;
            }

            BeginLevelTransition(_pendingLevelTransitionIndex);
        }

        private void TickTransitionClosing(float deltaTime)
        {
            _transitionTimer -= deltaTime;

            if (_transitionTimer <= 0f)
                ApplyLevelBehindCurtain();
        }

        private void TickTransitionOpening(float deltaTime)
        {
            _transitionTimer -= deltaTime;

            if (_transitionTimer <= 0f)
                FinishLevelTransition();
        }

        private void ExpireSession()
        {
            _runState = RunState.Expired;
            _shardDropService.DespawnAll();
            ApplySessionResults();
            OnSessionExpired?.Invoke();
        }

        private void ReachDemoLimit()
        {
            _runState = RunState.DemoLimitReached;
            _pendingLevelTransitionIndex = -1;
            _pendingDemoLimit = false;
            _lootGraceEndsAtDemoLimit = false;
            _lootGraceTimer = 0f;
            _shardDropService.DespawnAll();
            _transitionTargetLevelIndex = -1;
            ApplySessionResults();
            _dungeonSelection.MarkLevelReached(_currentDungeon, _currentLevelIndex);
            OnDemoLimitReached?.Invoke(_currentDungeon, _currentLevel, _currentLevelIndex);
        }

        private void ApplySessionResults()
        {
            if (_sessionResultsApplied)
                return;

            _player.GoldTotal += _sessionGold;
            _sessionRecordResult = _player.UpdateSessionRecords(_sessionGold, _sessionKills);
            _sessionResultsApplied = true;
        }

        private void RestartSessionFromCurrentLevel()
        {
            _spawnService.DespawnAll();
            _shardDropService.DespawnAll();
            ResetSessionStats();

            if (!ApplyLevel(_currentLevelIndex))
            {
                _runState = RunState.Inactive;
                return;
            }

            SpawnInitialEntities();
            _runState = RunState.Playing;
        }

        private void ResetSessionStats()
        {
            _sessionGold = BigDouble.Zero;
            _sessionKills = 0;
            _sessionRecordResult = default;
            _sessionResultsApplied = false;
            _pendingLevelTransitionIndex = -1;
            _pendingDemoLimit = false;
            _lootGraceEndsAtDemoLimit = false;
            _lootGraceTimer = 0f;

            OnSessionGoldEarned?.Invoke(_sessionGold, 0);
            OnSessionKillsChanged?.Invoke(_sessionKills);
        }

        private void BeginLevelTransition(int nextLevelIndex)
        {
            if (!_currentDungeon.TryGetLevel(nextLevelIndex, out var nextLevel) || nextLevel == null || !nextLevel.IsPlayable)
            {
                Debug.LogWarning($"[GameplayFeature] Cannot transition to dungeon level index {nextLevelIndex}.");
                _pendingLevelTransitionIndex = -1;
                return;
            }

            _runState = RunState.TransitionClosing;
            _pendingLevelTransitionIndex = -1;
            _pendingDemoLimit = false;
            _lootGraceEndsAtDemoLimit = false;
            _lootGraceTimer = 0f;
            _transitionTargetLevelIndex = nextLevelIndex;
            var transitionConfig = _levelTransitionConfig ?? new DungeonLevelTransitionConfig();
            _transitionTimer = transitionConfig.CloseDuration;
            _transitionHoldDuration = transitionConfig.HoldDuration;
            _transitionOpenDuration = transitionConfig.OpenDuration;

            OnLevelTransitionStarted?.Invoke(nextLevel, nextLevelIndex,
                _transitionTimer, _transitionHoldDuration, _transitionOpenDuration);

            if (_transitionTimer <= 0f)
                ApplyLevelBehindCurtain();
        }

        private void BeginLootGrace(int nextLevelIndex, bool endsAtDemoLimit)
        {
            _runState = RunState.LootGrace;
            _pendingLevelTransitionIndex = nextLevelIndex;
            _pendingDemoLimit = false;
            _lootGraceEndsAtDemoLimit = endsAtDemoLimit;
            _lootGraceTimer = (_levelTransitionConfig ?? new DungeonLevelTransitionConfig()).LootGraceDuration;

            if (_lootGraceTimer <= 0f)
            {
                if (_lootGraceEndsAtDemoLimit)
                    ReachDemoLimit();
                else
                    BeginLevelTransition(_pendingLevelTransitionIndex);
            }
        }

        private void ApplyLevelBehindCurtain()
        {
            var targetIndex = _transitionTargetLevelIndex;

            _spawnService.DespawnAll();
            _shardDropService.DespawnAll();

            if (!ApplyLevel(targetIndex))
            {
                _transitionTargetLevelIndex = -1;
                _transitionTimer = 0f;
                _transitionHoldDuration = 0f;
                _transitionOpenDuration = 0f;
                _runState = RunState.Inactive;
                return;
            }

            SpawnInitialEntities();
            _dungeonSelection.MarkLevelReached(_currentDungeon, _currentLevelIndex);

            _transitionTimer = Mathf.Max(0f, _transitionHoldDuration + _transitionOpenDuration);
            _transitionHoldDuration = 0f;
            _transitionOpenDuration = 0f;

            if (_transitionTimer <= 0f)
            {
                FinishLevelTransition();
                return;
            }

            _runState = RunState.TransitionOpening;
        }

        private void FinishLevelTransition()
        {
            _transitionTargetLevelIndex = -1;
            _transitionTimer = 0f;
            _transitionHoldDuration = 0f;
            _transitionOpenDuration = 0f;
            _runState = RunState.Playing;
            OnLevelTransitionFinished?.Invoke(_currentLevel, _currentLevelIndex);
        }

        private void HandleEnemyKilled(SpawnService.EntityDestroyedContext context)
        {
            if (_runState == RunState.Inactive || _runState == RunState.Expired || _runState == RunState.DemoLimitReached)
                return;

            var levelGoldMultiplier = _currentLevel != null ? _currentLevel.goldDropMultiplier : 1f;
            var goldDrop = context.Config != null ? context.Config.goldDrop : BigDouble.Zero;
            var finalAmount = BigDoubleMath.MultiplyAndRound(goldDrop,
                Mathf.Max(0f, levelGoldMultiplier * _skillTree.GetMultiplier(StatType.GoldDrop)));

            if (finalAmount > 0)
            {
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
            {
                _pendingLevelTransitionIndex = nextLevelIndex;
                return;
            }

            if (!_dungeonSelection.HasDemoEndAcknowledged(_currentDungeon))
                _pendingDemoLimit = true;
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
