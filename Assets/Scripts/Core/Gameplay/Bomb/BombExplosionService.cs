using System.Collections.Generic;
using Core.TestSkillTree;
using Entity;
using IncrementalRPG.Scripts.Core;
using UnityEngine;

namespace Core.Gameplay.Bomb
{
    public class BombExplosionService : IService
    {
        private readonly TileGrid _tileGrid;
        private readonly SpawnService _spawnService;
        private readonly BombExplosionConfig _config;
        private readonly SkillTreeService _skillTree;

        public BombExplosionService(TileGrid tileGrid, SpawnService spawnService, BombExplosionConfig config, SkillTreeService skillTree)
        {
            _tileGrid = tileGrid;
            _spawnService = spawnService;
            _config = config;
            _skillTree = skillTree;
        }

        public void Initialize()
        {
            _spawnService.OnFeatureSpawned += HandleFeatureSpawned;
        }

        public void Update(float deltaTime) { }

        private void HandleFeatureSpawned(Creature creature, Vector2Int coord, Entity.EntityConfig config)
        {
            if (config.featureType != Entity.FeatureType.Bomb) return;
            creature.OnDied += () => Explode(creature);
        }

        private void Explode(Creature source)
        {
            var epicenter = _tileGrid.GetWorldPosition(source.TileCoord);

            var a = _config.baseRadius * _skillTree.GetMultiplier(StatType.BombExplosionRadius)
                                       + _skillTree.GetBonus(StatType.BombExplosionRadius);
            var b = a * _config.aspectRatio;
            var damage = (int)((_config.baseDamage + _skillTree.GetBonus(StatType.BombExplosionDamage))
                               * _skillTree.GetMultiplier(StatType.BombExplosionDamage));

            // var debugGo = new GameObject("BombExplosionDebug");
            // debugGo.AddComponent<BombExplosionDebugView>().Show(epicenter, a, b, 0.5f);

            var targets = new List<Creature>(_tileGrid.GetAll());
            foreach (var creature in targets)
            {
                if (creature == source) continue;

                var pos = _tileGrid.GetWorldPosition(creature.TileCoord);
                var dx = (pos.x - epicenter.x) / a;
                var dy = (pos.y - epicenter.y) / b;
                if (dx * dx + dy * dy <= 1f)
                    creature.TakeDamage(damage);
            }
        }
    }
}
