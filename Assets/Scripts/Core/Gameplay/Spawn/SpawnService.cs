using System;
using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Core.TestSkillTree;
using Entity;
using IncrementalRPG.Scripts.Core;
using UnityEngine;

namespace Core.Gameplay
{
    public class SpawnService : IService
    {
        public event Action<Vector2Int, int> OnCreatureKilled;
        public event Action<Creature, Vector2Int, EntityConfig> OnFeatureSpawned;
        private readonly PoolManager _poolManager;
        private readonly TileGrid _tileGrid;
        private readonly SkillTreeService _skillTree;
        private SpawnTable _spawnTable;

        private float _spawnInterval = 2f;
        private float _timer;
        private readonly List<ActiveEntry> _active = new();

        private struct FeatureTimer
        {
            public FeatureType Type;
            public float Interval;
            public float Elapsed;
        }
        private readonly List<FeatureTimer> _featureTimers = new();

        private struct ActiveEntry
        {
            public Creature Creature;
            public CreatureView View;
            public EntityConfig Config;
            public Action OnDied;
        }

        public SpawnService(PoolManager poolManager, TileGrid tileGrid, SkillTreeService skillTree)
        {
            _poolManager = poolManager;
            _tileGrid = tileGrid;
            _skillTree = skillTree;
        }

        public void SetLevel(DungeonLevelConfig level)
        {
            _spawnTable = level.spawnTable;
            _spawnInterval = level.spawnInterval;
            _timer = 0f;
            _featureTimers.Clear();
        }

        public void SetSpawnInterval(float interval)
        {
            _spawnInterval = interval;
        }

        public void SetFeatureSpawnInterval(FeatureType featureType, float interval)
        {
            for (var i = 0; i < _featureTimers.Count; i++)
            {
                if (_featureTimers[i].Type != featureType) continue;
                var t = _featureTimers[i];
                t.Interval = interval;
                _featureTimers[i] = t;
                return;
            }
            _featureTimers.Add(new FeatureTimer { Type = featureType, Interval = interval });
        }

        public void Initialize() { }

        public void SpawnInitial(int count, FeatureType featureType = FeatureType.None)
        {
            for (var i = 0; i < count; i++)
                TrySpawnOfType(featureType);
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= _spawnInterval)
            {
                _timer = 0f;
                TrySpawnOfType(FeatureType.None);
            }

            for (var i = 0; i < _featureTimers.Count; i++)
            {
                var ft = _featureTimers[i];
                ft.Elapsed += deltaTime;
                if (ft.Elapsed >= ft.Interval)
                {
                    ft.Elapsed = 0f;
                    TrySpawnOfType(ft.Type);
                }
                _featureTimers[i] = ft;
            }
        }

        private void TrySpawnOfType(FeatureType featureType)
        {
            if (_spawnTable == null) return;
            if (!_tileGrid.TryGetRandomFreeTile(out var coord)) return;

            var config = _spawnTable.Pick(_skillTree, featureType);
            if (config == null) return;

            Spawn(config, coord);
        }

        public void DespawnAll()
        {
            var snapshot = new List<ActiveEntry>(_active);
            _active.Clear();
            _timer = 0f;
            for (var i = 0; i < _featureTimers.Count; i++)
            {
                var ft = _featureTimers[i];
                ft.Elapsed = 0f;
                _featureTimers[i] = ft;
            }
            foreach (var entry in snapshot)
            {
                entry.Creature.OnDied -= entry.OnDied;
                _tileGrid.Free(entry.Creature);
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

                if (config.featureType == FeatureType.None)
                    OnCreatureKilled?.Invoke(creature.TileCoord, config.goldDrop);

                view.PlayDeath(() =>
                {
                    _active.RemoveAll(e => e.Creature == creature);
                    _tileGrid.Free(creature);
                    _poolManager.Return(view, config);
                });
            };
            creature.OnDied += onDied;
            _active.Add(new ActiveEntry { Creature = creature, View = view, Config = config, OnDied = onDied });

            if (config.featureType != FeatureType.None)
                OnFeatureSpawned?.Invoke(creature, coord, config);
        }
    }
}
