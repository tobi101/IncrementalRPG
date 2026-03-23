using System;
using Entity;
using IncrementalRPG.Scripts.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Gameplay
{
    public class SpawnService : IService
    {
        private readonly PoolManager _poolManager;
        private readonly EntityConfig[] _configs;
        private readonly TileGrid _tileGrid;

        private float _spawnInterval = 2f;
        private float _timer;

        public SpawnService(PoolManager poolManager, EntityConfig[] configs, TileGrid tileGrid)
        {
            _poolManager = poolManager;
            _configs = configs;
            _tileGrid = tileGrid;
        }

        public void Initialize() { }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer < _spawnInterval) return;
            _timer = 0f;
            TrySpawn();
        }

        private void TrySpawn()
        {
            if (_configs.Length == 0) return;
            if (!_tileGrid.TryGetRandomFreeTile(out var coord)) return;

            var config = _configs[Random.Range(0, _configs.Length)];
            Spawn(config, coord);
        }

        private void Spawn(EntityConfig config, Vector2Int coord)
        {
            var creature = new Creature(config, coord);
            var view = _poolManager.Get(config);
            view.transform.position = _tileGrid.GetWorldPosition(coord) + view.FootOffset;
            view.Bind(creature);
            _tileGrid.Place(creature);

            Action onDied = null;
            onDied = () =>
            {
                creature.OnDied -= onDied;
                _tileGrid.Free(creature);
                view.Unbind();
                _poolManager.Return(view, config);
            };
            creature.OnDied += onDied;
        }
    }
}
