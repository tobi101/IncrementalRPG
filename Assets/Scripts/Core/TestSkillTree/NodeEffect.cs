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
        ZoneRadius = 0,
        ZoneDamage = 1,
        ManualAttackSpeed = 2,
        AutoAttackSpeed = 12,
        SpawnSpeed = 3,
        MapSize = 4,
        SessionTime = 5,
        BombExplosionRadius = 6,
        BombExplosionDamage = 7,
        BombSpawnSpeed = 8,
        GoldDrop = 9,
        InitialEnemySpawnDensity = 10,
        InitialBombSpawnDensity = 11,
        SlimeShardDrop = 13,
        SkeletonShardDrop = 14,
        DemonShardDrop = 15,
        CrystalShardDropBonus = 16,
        ShardPickupSpeed = 17,
    }

    public enum GameFeature
    {
        None = 0,
        Bombs = 1,
        AutoAttack = 2,
        SpecialAttack = 3,
        Shards = 4,
    }

    public enum NodeEffectType
    {
        Additive,
        Multiplicative,
        FeatureUnlock,
    }
}
