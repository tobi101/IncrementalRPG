using System;
using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Entity;
using IncrementalRPG.Scripts.Core;
using UnityEngine;

namespace Core.Gameplay
{
    public class SpawnService : IService
    {
        public event Action<Vector2Int, int> OnCreatureKilled;
        private readonly PoolManager _poolManager;
        private readonly TileGrid _tileGrid;
        private SpawnTable _spawnTable;

        private float _spawnInterval = 2f;
        private float _timer;
        private readonly List<ActiveEntry> _active = new();

        private struct ActiveEntry
        {
            public Creature Creature;
            public CreatureView View;
            public EntityConfig Config;
            public Action OnDied;
        }

        public SpawnService(PoolManager poolManager, TileGrid tileGrid)
        {
            _poolManager = poolManager;
            _tileGrid = tileGrid;
        }

        public void SetDungeon(DungeonConfig dungeon)
        {
            _spawnTable = dungeon.spawnTable;
            _spawnInterval = dungeon.spawnInterval;
        }

        public void SetSpawnInterval(float interval)
        {
            _spawnInterval = interval;
        }

        public void Initialize() { }

        public void SpawnInitial(int count)
        {
            for (var i = 0; i < count; i++)
                TrySpawn();
        }

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

        public void DespawnAll()
        {
            var snapshot = new List<ActiveEntry>(_active);
            _active.Clear();
            _timer = 0f;
            foreach (var entry in snapshot)
            {
                entry.Creature.OnDied -= entry.OnDied;
                _tileGrid.Free(entry.Creature);
                entry.View.Unbind();
                _poolManager.Return(entry.View, entry.Config);
            }
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
                _active.RemoveAll(e => e.Creature == creature);
                _tileGrid.Free(creature);
                view.Unbind();
                _poolManager.Return(view, config);

                if (config.goldDrop > 0)
                    OnCreatureKilled?.Invoke(creature.TileCoord, config.goldDrop);
            };
            creature.OnDied += onDied;
            _active.Add(new ActiveEntry { Creature = creature, View = view, Config = config, OnDied = onDied });
        }
    }
}
