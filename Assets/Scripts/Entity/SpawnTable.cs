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

        public EntityConfig Pick(SkillTreeService skillTree, FeatureType featureType = FeatureType.None)
        {
            if (entries == null || entries.Length == 0) return null;

            float total = 0f;
            foreach (var e in entries)
                if (e.config.featureType == featureType
                    && (e.requiredFeature == GameFeature.None || skillTree.IsUnlocked(e.requiredFeature)))
                    total += e.weight;

            if (total <= 0f) return null;

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            foreach (var e in entries)
            {
                if (e.config.featureType != featureType) continue;
                if (e.requiredFeature != GameFeature.None && !skillTree.IsUnlocked(e.requiredFeature)) continue;

                cumulative += e.weight;
                if (roll < cumulative)
                    return e.config;
            }

            return null;
        }
    }
}
