using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    public class DungeonSelectionService
    {
        private DungeonConfig _selectedDungeon;

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
    }
}
