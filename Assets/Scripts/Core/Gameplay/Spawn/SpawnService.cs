using System;
using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Core.TestSkillTree;
using Entity;
using IncrementalRPG.Scripts.AudioManager;
using IncrementalRPG.Scripts.Core;
using UnityEngine;
using Utils;

namespace Core.Gameplay
{
    public class SpawnService : IService
    {
        public readonly struct EntityDestroyedContext
        {
            public Creature Creature { get; }
            public EntityConfig Config { get; }
            public Vector2Int TileCoord { get; }
            public Vector3 WorldPosition { get; }

            public EntityDestroyedContext(Creature creature, EntityConfig config, Vector2Int tileCoord, Vector3 worldPosition)
            {
                Creature = creature;
                Config = config;
                TileCoord = tileCoord;
                WorldPosition = worldPosition;
            }
        }

        public event Action<EntityDestroyedContext> OnEntityDestroyed;
        public event Action<EntityDestroyedContext> OnEnemyKilled;
        public event Action<Creature, CreatureView, Vector2Int, EntityConfig> OnFeatureSpawned;
        private readonly PoolManager _poolManager;
        private readonly TileGrid _tileGrid;
        private readonly SkillTreeService _skillTree;
        private readonly AudioManager _audioManager;
        private readonly DamagePopupService _damagePopupService;
        private readonly EnemyMovementService _movementService;
        private SpawnTable _spawnTable;
        private EntityConfig _levelBoss;

        private float _spawnInterval = 2f;
        private float _timer;
        private float _bombSpawnInterval;
        private float _bombTimer;
        private readonly List<ActiveEntry> _active = new();
        private readonly List<Action> _pendingDeathCompletions = new();
        private bool _isPaused;
        private bool _areViewsPaused;
        private bool _areDeathAnimationsPaused;

        private struct ActiveEntry
        {
            public Creature Creature;
            public CreatureView View;
            public EntityConfig Config;
            public bool OccupiesTile;
            public Action OnDied;
            public Action<BigDouble> OnDamageTaken;
        }

        public SpawnService(PoolManager poolManager, TileGrid tileGrid, SkillTreeService skillTree, AudioManager audioManager,
            DamagePopupService damagePopupService, EnemyMovementService movementService)
        {
            _poolManager = poolManager;
            _tileGrid = tileGrid;
            _skillTree = skillTree;
            _audioManager = audioManager;
            _damagePopupService = damagePopupService;
            _movementService = movementService;
        }

        public void SetLevel(DungeonLevelConfig level)
        {
            _levelBoss = level.bossEncounter?.boss;
            _spawnTable = level.bossEncounter == null ? level.spawnTable : null;
            _spawnInterval = level.spawnInterval;
            _bombSpawnInterval = level.bombSpawnInterval;
            _timer = 0f;
            _bombTimer = 0f;
        }

        public void SetSpawnInterval(float interval)
        {
            _spawnInterval = interval;
        }

        public void SetBombSpawnInterval(float interval)
        {
            _bombSpawnInterval = interval;
        }

        public void Initialize() { }

        public void SpawnInitial(int count, FeatureType featureType = FeatureType.None)
        {
            for (var i = 0; i < count; i++)
                TrySpawnOfType(featureType);
        }

        public void Update(float deltaTime)
        {
            if (_isPaused || _spawnTable == null) return;
            _movementService.Update(deltaTime);
            _timer += deltaTime;
            if (_timer >= _spawnInterval)
            {
                _timer = 0f;
                TrySpawnNonBomb();
            }

            if (!_skillTree.IsUnlocked(GameFeature.Bombs)) return;

            _bombTimer += deltaTime;
            if (_bombTimer >= _bombSpawnInterval)
            {
                _bombTimer = 0f;
                TrySpawnOfType(FeatureType.Bomb);
            }
        }

        public IEnumerable<Creature> GetLivingCreatures()
        {
            foreach (var entry in _active)
                if (entry.Creature.IsAlive)
                    yield return entry.Creature;
        }

        internal CreatureView FindView(Creature creature)
        {
            foreach (var entry in _active)
                if (entry.Creature == creature)
                    return entry.View;

            return null;
        }

        public void SetPaused(bool isPaused)
        {
            if (_isPaused == isPaused)
                return;

            _isPaused = isPaused;
            SetViewsPaused(isPaused);

            if (!_isPaused)
                FlushPendingDeathCompletions();
        }

        public void SetViewsPaused(bool isPaused, bool pauseDeathAnimations = true)
        {
            var areDeathAnimationsPaused = isPaused && pauseDeathAnimations;
            if (_areViewsPaused == isPaused && _areDeathAnimationsPaused == areDeathAnimationsPaused) return;
            _areViewsPaused = isPaused;
            _areDeathAnimationsPaused = areDeathAnimationsPaused;
            foreach (var entry in _active)
                entry.View.SetPaused(entry.Creature.IsAlive ? _areViewsPaused : _areDeathAnimationsPaused);
        }

        private void TrySpawnNonBomb()
        {
            if (_spawnTable == null) return;
            if (!_tileGrid.TryGetRandomFreeTile(out var coord)) return;

            var config = _spawnTable.PickNonBomb(_skillTree);
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
            _pendingDeathCompletions.Clear();
            _timer = 0f;
            _bombTimer = 0f;
            foreach (var entry in snapshot)
            {
                entry.Creature.OnDied -= entry.OnDied;
                entry.Creature.OnDamageTaken -= entry.OnDamageTaken;
                RemoveFromGrid(entry.Creature, entry.OccupiesTile);
                _poolManager.Return(entry.View, entry.Config);
            }
        }

        private void Spawn(EntityConfig config, Vector2Int coord)
        {
            var creature = new Creature(config, coord);
            _tileGrid.Place(creature);
            _movementService.Register(creature);
            BindSpawnedCreature(creature, occupiesTile: true);
        }

        internal Creature SpawnAt(EntityConfig config, Vector3 worldPosition)
        {
            var creature = new Creature(config, default) { WorldPosition = worldPosition };
            BindSpawnedCreature(creature, occupiesTile: false);
            return creature;
        }

        private void BindSpawnedCreature(Creature creature, bool occupiesTile)
        {
            var config = creature.Config;
            var view = _poolManager.Get(config);
            view.Bind(creature);
            view.SetPaused(_areViewsPaused);

            Action<BigDouble> onDamageTaken = damage =>
            {
                if (view != null)
                    _damagePopupService.ShowDamage(damage, view.DamagePopupWorldPosition);
            };

            Action onDied = null;
            onDied = () =>
            {
                creature.OnDied -= onDied;
                creature.OnDamageTaken -= onDamageTaken;
                RemoveFromGrid(creature, occupiesTile);

                var destroyedContext = new EntityDestroyedContext(
                    creature,
                    config,
                    creature.TileCoord,
                    creature.WorldPosition);

                OnEntityDestroyed?.Invoke(destroyedContext);
                if (config.countsAsEnemyKill)
                    OnEnemyKilled?.Invoke(destroyedContext);

                // All four totems can use the frame's death sound budget before the boss dies.
                if (config == _levelBoss)
                    _audioManager?.PlayRandomSfx(config.deathSounds);
                else
                    _audioManager?.PlayRandomDeathSfx(config.deathSounds);
                Action completeDeath = () =>
                {
                    _active.RemoveAll(e => e.Creature == creature);
                    _poolManager.Return(view, config);
                };

                // A creature can die after its living animation was paused for a transition.
                view.SetPaused(_areDeathAnimationsPaused);
                view.PlayDeath(() => CompleteDeathOrDefer(completeDeath));
            };
            creature.OnDamageTaken += onDamageTaken;
            creature.OnDied += onDied;
            _active.Add(new ActiveEntry
            {
                Creature = creature,
                View = view,
                Config = config,
                OccupiesTile = occupiesTile,
                OnDied = onDied,
                OnDamageTaken = onDamageTaken
            });

            if (config.featureType != FeatureType.None)
                OnFeatureSpawned?.Invoke(creature, view, creature.TileCoord, config);
        }

        private void RemoveFromGrid(Creature creature, bool occupiesTile)
        {
            if (!occupiesTile) return;
            _movementService.Unregister(creature);
            _tileGrid.Free(creature);
        }

        private void CompleteDeathOrDefer(Action completeDeath)
        {
            if (!_isPaused)
            {
                completeDeath?.Invoke();
                return;
            }

            _pendingDeathCompletions.Add(completeDeath);
        }

        private void FlushPendingDeathCompletions()
        {
            if (_pendingDeathCompletions.Count == 0)
                return;

            var snapshot = new List<Action>(_pendingDeathCompletions);
            _pendingDeathCompletions.Clear();

            foreach (var completeDeath in snapshot)
                completeDeath?.Invoke();
        }
    }
}
