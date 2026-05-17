using System;
using Core.Save;
using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    public class DungeonSelectionService : ISaveable
    {
        public event Action OnProgressChanged;

        private DungeonConfig _selectedDungeon;
        private DungeonProgressState _progressState = new();

        public DungeonConfig SelectedDungeon => _selectedDungeon;

        public void Select(DungeonConfig dungeon)
        {
            _selectedDungeon = dungeon;
        }

        public DungeonConfig GetSelectedOrDefault(DungeonList dungeonList)
        {
            if (_selectedDungeon != null && _selectedDungeon.HasPlayableLevels)
                return _selectedDungeon;

            var fallback = dungeonList != null ? dungeonList.GetFirstPlayable() : null;
            if (fallback == null)
                Debug.LogError("[DungeonSelectionService] No playable dungeon is available.");

            _selectedDungeon = fallback;
            return _selectedDungeon;
        }

        public int GetStartLevelIndex(DungeonConfig dungeon)
        {
            if (dungeon == null || !dungeon.HasPlayableLevels)
                return -1;

            var savedLevelIndex = _progressState.GetReachedLevelIndex(GetProgressKey(dungeon));
            if (savedLevelIndex < 0)
                return dungeon.FirstPlayableLevelIndex;

            var clampedLevelIndex = Mathf.Clamp(savedLevelIndex, 0, dungeon.LevelCount - 1);
            if (dungeon.TryGetLevel(clampedLevelIndex, out var savedLevel) && savedLevel != null && savedLevel.IsPlayable)
                return clampedLevelIndex;

            Debug.LogWarning($"[DungeonSelectionService] Saved level index {savedLevelIndex} for dungeon '{dungeon.DisplayName}' is not playable. Falling back to first playable level.");
            return dungeon.FirstPlayableLevelIndex;
        }

        public void MarkLevelReached(DungeonConfig dungeon, int levelIndex)
        {
            if (dungeon == null || levelIndex < 0)
                return;

            if (!_progressState.SetReachedLevelIndex(GetProgressKey(dungeon), levelIndex))
                return;

            OnProgressChanged?.Invoke();
        }

        public void Load(SaveData data)
        {
            _progressState = data.DungeonProgressState ?? new DungeonProgressState();
        }

        public void Contribute(SaveData data)
        {
            data.DungeonProgressState = _progressState;
        }

        private static string GetProgressKey(DungeonConfig dungeon)
        {
            if (dungeon == null)
                return string.Empty;

            return string.IsNullOrEmpty(dungeon.dungeonId) ? dungeon.name : dungeon.dungeonId;
        }
    }
}
