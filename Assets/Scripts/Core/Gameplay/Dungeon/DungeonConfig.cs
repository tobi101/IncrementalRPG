using Entity;
using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonConfig", menuName = "RPG/Dungeon Config")]
    public class DungeonConfig : ScriptableObject
    {
        public SpawnTable spawnTable;
        public TilemapGenerationConfig tilemapGenerationConfig;
        [Min(0)] public int minPlayZoneSize;
        [Min(0)] public int initialSpawnCount;
    }
}
