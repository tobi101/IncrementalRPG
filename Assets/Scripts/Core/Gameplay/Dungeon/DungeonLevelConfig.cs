using Core.TestSkillTree;
using Entity;
using UnityEngine;
using UnityEngine.Localization;

namespace Core.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonLevelConfig", menuName = "RPG/Dungeon Level Config")]
    public class DungeonLevelConfig : ScriptableObject
    {
        [Header("Identity")]
        public string levelId;
        public LocalizedString displayName = new();
        public LocalizedString title = new();
        public LocalizedString description = new();

        [Header("Progression")]
        [Min(0)] public int killGoal = 10;

        [Header("Generation")]
        public TilemapGenerationConfig tilemapGenerationConfig;
        [Min(0)] public int minPlayZoneSize;
        [Min(0.1f)] public float heatIndex = 1f;

        [Header("Spawn")]
        public SpawnTable spawnTable;
        [Min(0f)] public float initialEnemySpawnDensity;
        [Min(0f)] public float initialBombSpawnDensity;
        [Min(0.1f)] public float spawnInterval = 2f;
        [Min(0.0000001f)] public float minSpawnInterval = 0.5f;
        public FeatureSpawnConfig[] featureSpawnConfigs = new FeatureSpawnConfig[0];

        [Header("Rewards")]
        [Min(0f)] public float goldDropMultiplier = 1f;

        public bool IsPlayable => spawnTable != null && tilemapGenerationConfig != null;
    }
}
