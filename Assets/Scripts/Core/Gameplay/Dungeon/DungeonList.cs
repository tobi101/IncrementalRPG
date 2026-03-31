using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonList", menuName = "RPG/Dungeon List")]
    public class DungeonList : ScriptableObject
    {
        public DungeonConfig[] dungeons;

        public DungeonConfig Get(int index) => dungeons[index];
        public int Count => dungeons.Length;
    }
}
