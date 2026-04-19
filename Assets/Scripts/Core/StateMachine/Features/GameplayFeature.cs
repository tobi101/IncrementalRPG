using System;
using System.Collections.Generic;
using System.Linq;
using Core.Gameplay;
using UnityEngine;
using Core.Gameplay.Dungeon;
using Core.TestSkillTree;
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

        private List<IService> _services;
        private DungeonConfig _currentDungeon;
        private float _sessionTimeLeft;
        private float _sessionTotalTime;
        private bool _isActive;
        private bool _isStarted;
        private BigDouble _sessionGold;
        private int _sessionKills;

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
            InitGameZone();
            _sessionTotalTime = (100f / _currentDungeon.heatIndex)
                              - (_player.ArmorIndex / 2.5f)
                              + _skillTree.GetBonus(StatType.SessionTime);
            _sessionTimeLeft = _sessionTotalTime;
            _generator.CameraAutoFitter.PrepareLavaAnimation();

            if (!_isStarted)
            {
                var zoneSize = _currentDungeon.initialSpawnCount + (int)_skillTree.GetBonus(StatType.SpawnCountMax);
                _spawnService.SpawnInitial(zoneSize);
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
                OnSessionExpired?.Invoke();

            foreach (var service in _services)
                service.Update(deltaTime);
        }

        private void HandleCreatureKilled(Vector2Int coord, int amount)
        {
            _player.GoldTotal += amount;
            _sessionGold += amount;
            _sessionKills++;
            OnSessionGoldEarned?.Invoke(_sessionGold, amount);
            OnSessionKillsChanged?.Invoke(_sessionKills);
        }

        private void InitGameZone()
        {
            _currentDungeon = _dungeonList.Get(0);
            _spawnService.SetDungeon(_currentDungeon);
            _spawnService.SetSpawnInterval(_currentDungeon.spawnInterval / _skillTree.GetMultiplier(StatType.SpawnSpeed));
            _generator.config = _currentDungeon.tilemapGenerationConfig;
            _generator.Size = _currentDungeon.minPlayZoneSize + (int)_skillTree.GetBonus(StatType.MapSize);
            _generator.Generate();
            _tileGrid.Initialize(_generator.TargetTilemap);
        }
    }
}
