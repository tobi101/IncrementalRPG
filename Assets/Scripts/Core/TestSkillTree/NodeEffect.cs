using System;
using UnityEngine;

namespace Core.TestSkillTree
{
    [Serializable]
    public class NodeEffect
    {
        public NodeEffectType effectType;

        [Tooltip("Which stat to modify. Ignored for FeatureUnlock.")]
        public StatType statType;

        [Tooltip("Bonus per upgrade level. Index 0 = level 1. Ignored for FeatureUnlock.")]
        public float[] valuesPerLevel;

        [Tooltip("Which feature to unlock. Ignored for Additive/Multiplicative.")]
        public GameFeature feature;
    }
    
    public enum StatType
    {
        ZoneRadius,
        ZoneDamage,
        AttackSpeed,
        SpawnSpeed,
        SpawnCountMax,
        MapSize,
        SessionTime
    }

    public enum GameFeature
    {
        Bombs,
    }

    public enum NodeEffectType
    {
        Additive,
        Multiplicative,
        FeatureUnlock,
    }
}
