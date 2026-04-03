using System.Collections.Generic;
using System.Linq;
using Core.Gameplay;
using Core.Gameplay.Dungeon;
using IncrementalRPG.Scripts.Core;
using IncrementalRPG.Scripts.Reflex;
using Model;
using Reflex.Attributes;


namespace Core
{
    public class GameLoop : IAwakeable, IStartable, ITickable
    {
        private readonly List<IService> _services;
        
        [Inject] private DungeonList _dungeonList;
        [Inject] private IsometricGradientTilemapGenerator _isometricTilemapGenerator;
        [Inject] private SpawnService _spawnService;
        [Inject] private TileGrid _tileGrid;
        [Inject] private Player _player;

        private DungeonConfig _currentDungeon;
        
        public GameLoop(IEnumerable<IService> systems)
        {
            _services = systems.ToList();
        }

        public void OnAwake()
        {
            InitServices();
            InitGameZone();
        }

        public void OnStart()
        {
            var zoneSize = _currentDungeon.initialSpawnCount + _player.StartSpawnObjectCount;
            _spawnService.SpawnInitial(zoneSize);
        }

        public void Tick(float deltaTime)
        {
            foreach (var t in _services)
                t.Update(deltaTime);
        }

        private void InitServices()
        {
            foreach (var t in _services)
                t.Initialize();
        }
        
        private void InitGameZone()
        {
            _currentDungeon = _dungeonList.Get(0);
            
            _spawnService.SetDungeon(_currentDungeon);
            
            _isometricTilemapGenerator.config = _currentDungeon.tilemapGenerationConfig;
            _isometricTilemapGenerator.Size = _currentDungeon.minPlayZoneSize;
            _isometricTilemapGenerator.Generate();
            
            _tileGrid.Initialize(_isometricTilemapGenerator.TargetTilemap);
        }

        private void InitDamageZone()
        {
            
        }
    }
}
