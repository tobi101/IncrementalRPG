using System.Collections.Generic;
using System.Linq;
using Core.Gameplay;
using Core.Gameplay.Dungeon;
using Core.TestSkillTree;
using IncrementalRPG.Scripts.Core;
using Model;
using Reflex.Attributes;

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

        private List<IService> _services;
        private DungeonConfig _currentDungeon;
        private bool _isActive;
        private bool _isStarted;

        public void Initialize()
        {
            _services = _servicesEnumerable.ToList();

            foreach (var service in _services)
                service.Initialize();
        }

        public void Enable()
        {
            _isActive = true;
            InitGameZone();

            if (!_isStarted)
            {
                var zoneSize = _currentDungeon.initialSpawnCount + _player.StartSpawnObjectCount + (int)_skillTree.GetBonus(StatType.SpawnCountMax);
                _spawnService.SpawnInitial(zoneSize);
                _isStarted = true;
            }
        }

        public void Disable()
        {
            _isActive = false;
            _isStarted = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_isActive) return;

            foreach (var service in _services)
                service.Update(deltaTime);
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
