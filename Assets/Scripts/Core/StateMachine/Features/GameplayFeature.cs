using System;
using System.Collections.Generic;
using System.Linq;
using Core.Gameplay;
using UnityEngine;
using Core.Gameplay.Dungeon;
using Core.TestSkillTree;
using Entity;
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
        [Inject] private IsometricGradientTilemapGenerator _generator;
        [Inject] private SpawnService _spawnService;
        [Inject] private TileGrid _tileGrid;
        [Inject] private Player _player;
        [Inject] private SkillTreeService _skillTree;

        public event Action OnSessionExpired;
        public event Action<BigDouble, int> OnSessionGoldEarned;
        public event Action<int> OnSessionKillsChanged;

        public BigDouble SessionGold => _sessionGold;
        public int SessionKills => _sessionKills;
        public SessionRecordResult SessionRecordResult => _sessionRecordResult;

        private List<IService> _services;
        private DungeonConfig _currentDungeon;
        private float _sessionTimeLeft;
        private float _sessionTotalTime;
        private bool _isActive;
        private bool _isStarted;
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
            _isActive = true;
            _sessionGold = BigDouble.Zero;
            _sessionKills = 0;
            _sessionRecordResult = default;
            InitGameZone();
            _sessionTotalTime = (100f / _currentDungeon.heatIndex)
                              - (_player.ArmorIndex / 2.5f)
                              + _skillTree.GetBonus(StatType.SessionTime);
            _sessionTimeLeft = _sessionTotalTime;
            _generator.CameraAutoFitter.PrepareLavaAnimation();

            if (!_isStarted)
            {
                SpawnInitialEntities();
                _isStarted = true;
            }
        }

        public void Disable()
        {
            _isActive = false;
            _isStarted = false;
            _spawnService.DespawnAll();
        }

        public void Tick(float deltaTime)
        {
            if (!_isActive) return;

            _sessionTimeLeft -= deltaTime;
            var progress = 1f - Mathf.Clamp01(_sessionTimeLeft / _sessionTotalTime);
            _generator.CameraAutoFitter.AnimateLava(progress);

            if (_sessionTimeLeft <= 0)
            {
                _isActive = false;
                _sessionRecordResult = _player.UpdateSessionRecords(_sessionGold, _sessionKills);
                OnSessionExpired?.Invoke();
            }

            foreach (var service in _services)
                service.Update(deltaTime);
        }

        private void HandleCreatureKilled(Vector2Int coord, int amount)
        {
            var finalAmount = Mathf.RoundToInt(amount * _skillTree.GetMultiplier(StatType.GoldDrop));

            _player.GoldTotal += finalAmount;
            _sessionGold += finalAmount;
            _sessionKills++;
            OnSessionGoldEarned?.Invoke(_sessionGold, finalAmount);
            OnSessionKillsChanged?.Invoke(_sessionKills);
        }

        private void SpawnInitialEntities()
        {
            var totalTiles = _tileGrid.TotalTileCount;
            var enemyDensity = Mathf.Max(0f, _currentDungeon.initialEnemySpawnDensity
                                             + _skillTree.GetBonus(StatType.InitialEnemySpawnDensity));
            var bombDensity = Mathf.Max(0f, _currentDungeon.initialBombSpawnDensity
                                            + _skillTree.GetBonus(StatType.InitialBombSpawnDensity));

            var enemyCount = Mathf.RoundToInt(totalTiles * enemyDensity);
            var bombCount = Mathf.RoundToInt(totalTiles * bombDensity);

            _spawnService.SpawnInitial(enemyCount);
            _spawnService.SpawnInitial(bombCount, FeatureType.Bomb);
        }

        private void InitGameZone()
        {
            _currentDungeon = _dungeonList.Get(0);
            _spawnService.SetDungeon(_currentDungeon);
            var spawnInterval = _currentDungeon.spawnInterval * (1f - _skillTree.GetBonus(StatType.SpawnSpeed));
            _spawnService.SetSpawnInterval(Mathf.Max(spawnInterval, _currentDungeon.minSpawnInterval));

            foreach (var fc in _currentDungeon.featureSpawnConfigs)
            {
                var featureInterval = fc.spawnInterval * (1f - _skillTree.GetBonus(fc.spawnSpeedStat));
                _spawnService.SetFeatureSpawnInterval(fc.featureType, Mathf.Max(featureInterval, fc.minSpawnInterval));
            }
            _generator.config = _currentDungeon.tilemapGenerationConfig;
            _generator.Size = _currentDungeon.minPlayZoneSize + (int)_skillTree.GetBonus(StatType.MapSize);
            _generator.Generate();
            _tileGrid.Initialize(_generator.TargetTilemap);
        }
    }
}
