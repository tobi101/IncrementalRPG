using System;
using Entity;
using IncrementalRPG.Scripts.Core;
using UnityEngine;

namespace Core.Gameplay
{
    public class SpawnService : IService
    {
        public event Action<Vector2Int, int> OnCreatureKilled;
        private readonly PoolManager _poolManager;
        private readonly SpawnTable _spawnTable;
        private readonly TileGrid _tileGrid;

        private float _spawnInterval = 2f;
        private float _timer;

        public SpawnService(PoolManager poolManager, SpawnTable spawnTable, TileGrid tileGrid)
        {
            _poolManager = poolManager;
            _spawnTable = spawnTable;
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
            if (!_tileGrid.TryGetRandomFreeTile(out var coord)) return;

            var config = _spawnTable.Pick();
            if (config == null) return;

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

                if (config.goldDrop > 0)
                    OnCreatureKilled?.Invoke(creature.TileCoord, config.goldDrop);
            };
            creature.OnDied += onDied;
        }
    }
}
