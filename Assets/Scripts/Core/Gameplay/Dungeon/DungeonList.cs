using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonList", menuName = "RPG/Dungeon List")]
    public class DungeonList : ScriptableObject
    {
        public DungeonConfig[] dungeons;

        public DungeonConfig Get(int index) => dungeons[index];
        public int Count => dungeons == null ? 0 : dungeons.Length;

        public DungeonConfig GetFirstPlayable()
        {
            if (dungeons == null) return null;

            foreach (var dungeon in dungeons)
                if (dungeon != null && dungeon.HasPlayableLevels)
                    return dungeon;

            return null;
        }
    }
}
