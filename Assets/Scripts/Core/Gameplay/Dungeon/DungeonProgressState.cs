using System;
using System.Collections.Generic;

namespace Core.Gameplay.Dungeon
{
    [Serializable]
    public class DungeonProgressState
    {
        public List<DungeonProgressEntry> entries = new();

        public int GetReachedLevelIndex(string dungeonId)
        {
            if (string.IsNullOrEmpty(dungeonId) || entries == null)
                return -1;

            foreach (var entry in entries)
            {
                if (entry != null && entry.dungeonId == dungeonId)
                    return entry.reachedLevelIndex;
            }

            return -1;
        }

        public bool SetReachedLevelIndex(string dungeonId, int levelIndex)
        {
            if (string.IsNullOrEmpty(dungeonId) || levelIndex < 0)
                return false;

            entries ??= new List<DungeonProgressEntry>();

            foreach (var entry in entries)
            {
                if (entry == null || entry.dungeonId != dungeonId) continue;
                if (entry.reachedLevelIndex >= levelIndex) return false;

                entry.reachedLevelIndex = levelIndex;
                return true;
            }

            entries.Add(new DungeonProgressEntry
            {
                dungeonId = dungeonId,
                reachedLevelIndex = levelIndex
            });

            return true;
        }
    }

    [Serializable]
    public class DungeonProgressEntry
    {
        public string dungeonId;
        public int reachedLevelIndex;
    }
}
