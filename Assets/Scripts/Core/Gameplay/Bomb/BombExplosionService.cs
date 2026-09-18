using System.Collections.Generic;
using Core.TestSkillTree;
using Core.Items;
using Entity;
using IncrementalRPG.Scripts.Core;
using UnityEngine;
using Utils;

namespace Core.Gameplay.Bomb
{
    public class BombExplosionService : IService
    {
        private readonly TileGrid _tileGrid;
        private readonly SpawnService _spawnService;
        private readonly BombExplosionConfig _config;
        private readonly SkillTreeService _skillTree;
        private readonly PlayerItemStorage _equipment;

        public BombExplosionService(TileGrid tileGrid, SpawnService spawnService, BombExplosionConfig config, SkillTreeService skillTree, PlayerItemStorage equipment)
        {
            _tileGrid = tileGrid;
            _spawnService = spawnService;
            _config = config;
            _skillTree = skillTree;
            _equipment = equipment;
        }

        public void Initialize()
        {
            _spawnService.OnFeatureSpawned += HandleFeatureSpawned;
        }

        public void Update(float deltaTime) { }

        private void HandleFeatureSpawned(Creature creature, CreatureView view, Vector2Int coord, Entity.EntityConfig config)
        {
            if (config.featureType != Entity.FeatureType.Bomb) return;

            ScaleExplosionVisual(view);
            creature.OnDied += () =>
            {
                ScaleExplosionVisual(view);
                Explode(creature);
            };
        }

        private void Explode(Creature source)
        {
            var epicenter = source.WorldPosition;

            var a = GetRadius();
            var b = a * _config.aspectRatio;
            var damage = GetDamage();

            // var debugGo = new GameObject("BombExplosionDebug");
            // debugGo.AddComponent<BombExplosionDebugView>().Show(epicenter, a, b, 0.5f);

            var targets = new List<Creature>(_tileGrid.GetAll());
            foreach (var creature in targets)
            {
                if (creature == source) continue;

                var pos = creature.WorldPosition;
                var dx = (pos.x - epicenter.x) / a;
                var dy = (pos.y - epicenter.y) / b;
                if (dx * dx + dy * dy <= 1f)
                    creature.TakeDamage(damage);
            }
        }

        public BigDouble GetDamage()
        {
            var trained = BigDouble.Max(BigDouble.Zero, _config.baseDamage + _skillTree.GetBonus(StatType.BombExplosionDamage));
            return BigDoubleMath.MultiplyAndRound(trained,
                (double)Mathf.Max(0f, _skillTree.GetMultiplier(StatType.BombExplosionDamage)) *
                _equipment.GetEquipmentMultiplier(EquipmentStats.BombDamage));
        }

        public float GetRadius() => (_config.baseRadius * _skillTree.GetMultiplier(StatType.BombExplosionRadius)
                   + _skillTree.GetBonus(StatType.BombExplosionRadius)) *
                   _equipment.GetEquipmentMultiplier(EquipmentStats.BombRadius);

        private void ScaleExplosionVisual(CreatureView view)
        {
            if (view == null) return;

            var visualScaler = view.GetComponentInChildren<BombExplosionVisualScaler>(true);
            if (visualScaler == null) return;

            visualScaler.ScaleToRadius(GetRadius(), _config.baseRadius);
        }
    }
}
