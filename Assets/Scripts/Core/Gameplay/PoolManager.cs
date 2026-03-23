using System.Collections.Generic;
using Entity;
using IncrementalRPG.Scripts.Core;
using UnityEngine;
using Utils;

namespace Core.Gameplay
{
    public class PoolManager : IService
    {
        private readonly Dictionary<EntityConfig, ObjectPool<CreatureView>> _pools = new();
        private Transform _poolRoot;

        public void Initialize()
        {
            _poolRoot = new GameObject("[Pool]").transform;
        }

        public void Update(float deltaTime) { }

        public CreatureView Get(EntityConfig config)
        {
            if (!_pools.TryGetValue(config, out var pool))
            {
                var prefab = config.viewPrefab.GetComponent<CreatureView>();
                if (prefab == null)
                {
                    Debug.LogError($"[PoolManager] Prefab '{config.viewPrefab.name}' on config '{config.entityName}' is missing CreatureView component.");
                    return null;
                }
                pool = new ObjectPool<CreatureView>(prefab, _poolRoot);
                _pools[config] = pool;
            }
            return pool.Get();
        }

        public void Return(CreatureView view, EntityConfig config)
        {
            if (_pools.TryGetValue(config, out var pool))
                pool.Return(view);
        }
    }
}
