using System;
using Core.TestSkillTree;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entity
{
    [Serializable]
    public class SpawnEntry
    {
        public EntityConfig config;
        [Min(0f)] public float weight = 1f;
        public GameFeature requiredFeature = GameFeature.None;
    }

    [CreateAssetMenu(fileName = "SpawnTable", menuName = "RPG/Spawn Table")]
    public class SpawnTable : ScriptableObject
    {
        public SpawnEntry[] entries;

        public EntityConfig PickAny(SkillTreeService skillTree)
        {
            return PickMatching(skillTree, null);
        }

        public EntityConfig Pick(SkillTreeService skillTree, FeatureType featureType = FeatureType.None)
        {
            return PickOfType(skillTree, featureType);
        }

        public EntityConfig PickOfType(SkillTreeService skillTree, FeatureType featureType)
        {
            return PickMatching(skillTree, featureType);
        }

        private EntityConfig PickMatching(SkillTreeService skillTree, FeatureType? featureType)
        {
            if (entries == null || entries.Length == 0) return null;

            float total = 0f;
            foreach (var e in entries)
            {
                if (!IsMatching(e, skillTree, featureType)) continue;

                var weight = GetEffectiveWeight(e, skillTree);
                if (weight > 0f)
                    total += weight;
            }

            if (total <= 0f) return null;

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            foreach (var e in entries)
            {
                if (!IsMatching(e, skillTree, featureType)) continue;

                var weight = GetEffectiveWeight(e, skillTree);
                if (weight <= 0f) continue;

                cumulative += weight;
                if (roll < cumulative)
                    return e.config;
            }

            return null;
        }

        private static bool IsMatching(SpawnEntry entry, SkillTreeService skillTree, FeatureType? featureType)
        {
            if (entry?.config == null) return false;
            if (featureType.HasValue && entry.config.featureType != featureType.Value) return false;
            return entry.requiredFeature == GameFeature.None
                   || (skillTree != null && skillTree.IsUnlocked(entry.requiredFeature));
        }

        private static float GetEffectiveWeight(SpawnEntry entry, SkillTreeService skillTree)
        {
            var weight = entry.weight;

            if (skillTree != null && entry.config.featureType == FeatureType.Bomb)
                weight *= Mathf.Max(0f, 1f + skillTree.GetBonus(StatType.BombSpawnSpeed));

            return weight;
        }
    }
}
