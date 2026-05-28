using System;
using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Core.TestSkillTree;
using Entity;
using IncrementalRPG.Scripts.AudioManager;
using IncrementalRPG.Scripts.Core;
using UnityEngine;

namespace Core.Gameplay
{
    public class SpawnService : IService
    {
        public event Action<Vector2Int, int> OnCreatureKilled;
        public event Action<Creature, CreatureView, Vector2Int, EntityConfig> OnFeatureSpawned;
        private readonly PoolManager _poolManager;
        private readonly TileGrid _tileGrid;
        private readonly SkillTreeService _skillTree;
        private readonly AudioManager _audioManager;
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

        public SpawnService(PoolManager poolManager, TileGrid tileGrid, SkillTreeService skillTree, AudioManager audioManager)
        {
            _poolManager = poolManager;
            _tileGrid = tileGrid;
            _skillTree = skillTree;
            _audioManager = audioManager;
        }

        public void SetLevel(DungeonLevelConfig level)
        {
            _spawnTable = level.spawnTable;
            _spawnInterval = level.spawnInterval;
            _timer = 0f;
        }

        public void SetSpawnInterval(float interval)
        {
            _spawnInterval = interval;
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
                TrySpawnAny();
            }
        }

        private void TrySpawnAny()
        {
            if (_spawnTable == null) return;
            if (!_tileGrid.TryGetRandomFreeTile(out var coord)) return;

            var config = _spawnTable.PickAny(_skillTree);
            if (config == null) return;

            Spawn(config, coord);
        }

        private void TrySpawnOfType(FeatureType featureType)
        {
            if (_spawnTable == null) return;
            if (!_tileGrid.TryGetRandomFreeTile(out var coord)) return;

            var config = _spawnTable.PickOfType(_skillTree, featureType);
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

                _audioManager?.PlayRandomSfx(config.deathSounds);
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
                OnFeatureSpawned?.Invoke(creature, view, coord, config);
        }
    }
}
