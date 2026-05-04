using System;
using Core.TestSkillTree;
using Entity;
using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    [Serializable]
    public class FeatureSpawnConfig
    {
        public FeatureType featureType;
        public StatType spawnSpeedStat;
        [Min(0.1f)] public float spawnInterval = 5f;
        [Min(0.0000001f)] public float minSpawnInterval = 1f;
    }

    [CreateAssetMenu(fileName = "DungeonConfig", menuName = "RPG/Dungeon Config")]
    public class DungeonConfig : ScriptableObject
    {
        public SpawnTable spawnTable;
        public TilemapGenerationConfig tilemapGenerationConfig;
        [Min(0)] public int minPlayZoneSize;
        [Min(0f)] public float initialEnemySpawnDensity;
        [Min(0f)] public float initialBombSpawnDensity;
        [Min(0.1f)] public float spawnInterval = 2f;
        [Min(0.0000001f)] public float minSpawnInterval = 0.5f;
        [Min(0.1f)] public float heatIndex = 1f;
        public FeatureSpawnConfig[] featureSpawnConfigs;
    }
}
