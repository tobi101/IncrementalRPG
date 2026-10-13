using System;
using System.Collections.Generic;

namespace Core.Gameplay.Dungeon
{
    [Serializable]
    public class DungeonProgressState
    {
        public List<DungeonProgressEntry> entries = new();

        public bool IsCompleted(string dungeonId) => !string.IsNullOrEmpty(dungeonId) &&
            entries != null && entries.Exists(e => e != null && e.dungeonId == dungeonId && e.completed);

        public bool SetCompleted(string dungeonId)
        {
            if (string.IsNullOrEmpty(dungeonId) || IsCompleted(dungeonId)) return false;
            entries ??= new List<DungeonProgressEntry>();
            var entry = entries.Find(e => e != null && e.dungeonId == dungeonId);
            if (entry == null) entries.Add(entry = new DungeonProgressEntry { dungeonId = dungeonId, reachedLevelIndex = -1 });
            entry.completed = true;
            return true;
        }

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

        public bool HasDemoEndAcknowledged(string dungeonId)
        {
            if (string.IsNullOrEmpty(dungeonId) || entries == null)
                return false;

            foreach (var entry in entries)
            {
                if (entry != null && entry.dungeonId == dungeonId)
                    return entry.demoEndAcknowledged;
            }

            return false;
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

        public bool SetDemoEndAcknowledged(string dungeonId)
        {
            if (string.IsNullOrEmpty(dungeonId))
                return false;

            entries ??= new List<DungeonProgressEntry>();

            foreach (var entry in entries)
            {
                if (entry == null || entry.dungeonId != dungeonId) continue;
                if (entry.demoEndAcknowledged) return false;

                entry.demoEndAcknowledged = true;
                return true;
            }

            entries.Add(new DungeonProgressEntry
            {
                dungeonId = dungeonId,
                reachedLevelIndex = -1,
                demoEndAcknowledged = true
            });

            return true;
        }
    }

    [Serializable]
    public class DungeonProgressEntry
    {
        public string dungeonId;
        public int reachedLevelIndex;
        public bool demoEndAcknowledged;
        public bool completed;
    }
}
