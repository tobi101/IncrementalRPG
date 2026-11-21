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
        private readonly SpawnService _spawnService;
        private readonly BombExplosionConfig _config;
        private readonly SkillTreeService _skillTree;
        private readonly PlayerItemStorage _equipment;

        public BombExplosionService(SpawnService spawnService, BombExplosionConfig config, SkillTreeService skillTree, PlayerItemStorage equipment)
        {
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

            var visual = view != null ? view.GetComponentInChildren<BombExplosionVisualScaler>(true) : null;
            if (visual == null)
            {
                Debug.LogError($"[BombExplosionService] Bomb '{config.name}' is missing its explosion visual.", config);
                return;
            }

            visual.ScaleToRadius(GetRadius(), _config.aspectRatio, creature.WorldPosition);
            creature.OnDied += HandleDeath;

            void HandleDeath()
            {
                creature.OnDied -= HandleDeath;
                var epicenter = creature.WorldPosition;
                var radius = GetRadius();
                var damage = GetDamage();
                visual.ScaleToRadius(radius, _config.aspectRatio, epicenter);

                // SpawnService's earlier OnDied handler has already started the death tracks.
                visual.WhenFullRadius(() => Explode(epicenter, radius, damage));
            }
        }

        private void Explode(Vector3 epicenter, float radius, BigDouble damage)
        {
            var a = radius;
            var b = a * _config.aspectRatio;

            // var debugGo = new GameObject("BombExplosionDebug");
            // debugGo.AddComponent<BombExplosionDebugView>().Show(epicenter, a, b, 0.5f);

            var targets = new List<Creature>(_spawnService.GetLivingCreatures());
            foreach (var creature in targets)
            {
                var pos = creature.WorldPosition;
                var dx = (pos.x - epicenter.x) / a;
                var dy = (pos.y - epicenter.y) / b;
                if (dx * dx + dy * dy <= 1f)
                    creature.TakeDamage(damage);
            }
        }

        public BigDouble GetDamage(NodeDefinition previewUpgrade = null)
        {
            var trained = BigDouble.Max(BigDouble.Zero, _config.baseDamage + _skillTree.GetBonus(StatType.BombExplosionDamage, previewUpgrade));
            return BigDoubleMath.MultiplyAndRound(trained,
                (double)Mathf.Max(0f, _skillTree.GetMultiplier(StatType.BombExplosionDamage, previewUpgrade)) *
                _equipment.GetEquipmentMultiplier(EquipmentStats.BombDamage));
        }

        public float GetRadius(NodeDefinition previewUpgrade = null) => (_config.baseRadius * _skillTree.GetMultiplier(StatType.BombExplosionRadius, previewUpgrade)
                   + _skillTree.GetBonus(StatType.BombExplosionRadius, previewUpgrade)) *
                   _equipment.GetEquipmentMultiplier(EquipmentStats.BombRadius);
    }
}
