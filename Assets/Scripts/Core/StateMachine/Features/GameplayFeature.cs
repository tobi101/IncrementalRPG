using System.Collections.Generic;
using System.Linq;
using Core.Gameplay;
using Core.Gameplay.Dungeon;
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

        private List<IService> _services;
        private DungeonConfig _currentDungeon;
        private bool _isActive;
        private bool _isStarted;

        public void Initialize()
        {
            _services = _servicesEnumerable.ToList();

            foreach (var service in _services)
                service.Initialize();

            InitGameZone();
        }

        public void Enable()
        {
            _isActive = true;

            if (!_isStarted)
            {
                var zoneSize = _currentDungeon.initialSpawnCount + _player.StartSpawnObjectCount;
                _spawnService.SpawnInitial(zoneSize);
                _isStarted = true;
            }
        }

        public void Disable() => _isActive = false;

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
            _generator.config = _currentDungeon.tilemapGenerationConfig;
            _generator.Size = _currentDungeon.minPlayZoneSize;
            _generator.Generate();
            _tileGrid.Initialize(_generator.TargetTilemap);
        }
    }
}
