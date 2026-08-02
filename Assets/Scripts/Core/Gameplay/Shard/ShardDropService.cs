using System;
using System.Collections.Generic;
using Core.TestSkillTree;
using Entity;
using IncrementalRPG.Scripts.Core;
using Model;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Core.Gameplay.Shards
{
    public sealed class ShardDropService : IService
    {
        private const float MinPickupSpeedMultiplier = 0.01f;

        private sealed class ActiveShard
        {
            public ShardPickupView View;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 Position;
            public int Value;
            public float ScatterElapsed;
            public float LifetimeRemaining;
            public float CollectionElapsed;
            public float ElapsedLifetime;
            public bool IsSettled;
        }

        private readonly SpawnService _spawnService;
        private readonly DamageZone _damageZone;
        private readonly ShardPickupConfig _config;
        private readonly SkillTreeService _skillTree;
        private readonly Player _player;
        private readonly List<ActiveShard> _active = new();

        private ObjectPool<ShardPickupView> _pool;
        private Transform _poolRoot;

        public int ActiveCount => _active.Count;

        public event Action<int, Vector3> OnShardCollected;

        public ShardDropService(SpawnService spawnService, DamageZone damageZone, ShardPickupConfig config,
            SkillTreeService skillTree, Player player)
        {
            _spawnService = spawnService;
            _damageZone = damageZone;
            _config = config;
            _skillTree = skillTree;
            _player = player;
        }

        public void Initialize()
        {
            _spawnService.OnEntityDestroyed += HandleEntityDestroyed;

            if (_config == null || _config.pickupPrefab == null)
            {
                Debug.LogError("[ShardDropService] ShardPickupConfig or pickup prefab is missing.");
                return;
            }

            _poolRoot = new GameObject("[ShardPool]").transform;
            _pool = new ObjectPool<ShardPickupView>(_config.pickupPrefab, _poolRoot);
        }

        public void Update(float deltaTime)
        {
            if (_active.Count == 0 || deltaTime <= 0f)
                return;

            // This keeps the overlap query current even in the level-complete loot grace state.
            _damageZone.UpdateAim();

            var pickupDuration = GetPickupDuration();
            var collectedValue = 0;
            var collectedPositions = new List<(int value, Vector3 position)>();

            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var shard = _active[i];
                shard.ElapsedLifetime += deltaTime;

                if (!shard.IsSettled)
                    UpdateScatter(shard, deltaTime);
                else
                    UpdateCollection(shard, deltaTime, pickupDuration);

                var collectionProgress = pickupDuration <= 0f
                    ? 1f
                    : Mathf.Clamp01(shard.CollectionElapsed / pickupDuration);
                shard.View.SetVisualProgress(collectionProgress, shard.ElapsedLifetime);

                // Collection wins if collection and expiration happen during the same frame.
                if (shard.IsSettled && shard.CollectionElapsed >= pickupDuration)
                {
                    collectedValue += shard.Value;
                    collectedPositions.Add((shard.Value, shard.Position));
                    ReturnAt(i);
                    continue;
                }

                shard.LifetimeRemaining -= deltaTime;
                if (shard.LifetimeRemaining <= 0f)
                    ReturnAt(i);
            }

            if (collectedValue <= 0)
                return;

            _player.AddShards(collectedValue);
            foreach (var collected in collectedPositions)
                OnShardCollected?.Invoke(collected.value, collected.position);
        }

        public void DespawnAll()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
                ReturnAt(i);
        }

        private void HandleEntityDestroyed(SpawnService.EntityDestroyedContext context)
        {
            var entityConfig = context.Config;
            if (!_skillTree.IsUnlocked(GameFeature.Shards)
                || entityConfig == null
                || entityConfig.shardDrop <= 0
                || _pool == null)
                return;

            var finalDrop = GetFinalDrop(entityConfig);
            var values = ShardDropMath.BuildPickupValues(
                entityConfig.shardDrop,
                _config.basePickupValue,
                finalDrop);

            foreach (var value in values)
                Spawn(value, context.WorldPosition);
        }

        private int GetFinalDrop(EntityConfig entityConfig)
        {
            var baseDrop = Mathf.Max(0, entityConfig.shardDrop);

            switch (entityConfig.entityKind)
            {
                case EntityKind.Slime:
                    return Mathf.Max(0, Mathf.RoundToInt(baseDrop * _skillTree.GetMultiplier(StatType.SlimeShardDrop)));
                case EntityKind.Skeleton:
                    return Mathf.Max(0, Mathf.RoundToInt(baseDrop * _skillTree.GetMultiplier(StatType.SkeletonShardDrop)));
                case EntityKind.Demon:
                    return Mathf.Max(0, Mathf.RoundToInt(baseDrop * _skillTree.GetMultiplier(StatType.DemonShardDrop)));
                case EntityKind.Crystal:
                    return Mathf.Max(0, Mathf.RoundToInt(baseDrop + _skillTree.GetBonus(StatType.CrystalShardDropBonus)));
                default:
                    return baseDrop;
            }
        }

        private float GetPickupDuration()
        {
            var additiveSpeed = 1f + _skillTree.GetBonus(StatType.ShardPickupSpeed);
            var multiplicativeSpeed = _skillTree.GetMultiplier(StatType.ShardPickupSpeed);
            var speedMultiplier = Mathf.Max(MinPickupSpeedMultiplier, additiveSpeed * multiplicativeSpeed);
            return Mathf.Max(0.01f, _config.baseCollectionDuration / speedMultiplier);
        }

        private void Spawn(int value, Vector3 origin)
        {
            if (value <= 0)
                return;

            var angle = Random.Range(0f, Mathf.PI * 2f);
            var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            var distance = Random.Range(_config.minScatterDistance, _config.maxScatterDistance);
            var view = _pool.Get();
            view.Prepare(origin);

            _active.Add(new ActiveShard
            {
                View = view,
                StartPosition = origin,
                EndPosition = origin + direction * distance,
                Position = origin,
                Value = value,
                LifetimeRemaining = _config.lifetime,
                IsSettled = _config.scatterDuration <= 0f
            });
        }

        private void UpdateScatter(ActiveShard shard, float deltaTime)
        {
            shard.ScatterElapsed += deltaTime;
            var duration = Mathf.Max(0.0001f, _config.scatterDuration);
            var t = Mathf.Clamp01(shard.ScatterElapsed / duration);
            var easedT = 1f - (1f - t) * (1f - t);
            shard.Position = Vector3.LerpUnclamped(shard.StartPosition, shard.EndPosition, easedT);
            shard.View.SetWorldPosition(shard.Position);

            if (t >= 1f)
                shard.IsSettled = true;
        }

        private void UpdateCollection(ActiveShard shard, float deltaTime, float pickupDuration)
        {
            if (_damageZone.ContainsWorldCircle(shard.Position, _config.hitRadius))
                shard.CollectionElapsed = Mathf.Min(pickupDuration, shard.CollectionElapsed + deltaTime);
            else
                shard.CollectionElapsed = 0f;
        }

        private void ReturnAt(int index)
        {
            var shard = _active[index];
            _active.RemoveAt(index);
            shard.View.ResetForPool();
            _pool.Return(shard.View);
        }
    }
}
