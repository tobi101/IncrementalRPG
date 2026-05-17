using UnityEngine;
using UnityEngine.Localization;

namespace Core.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonConfig", menuName = "RPG/Dungeon Config")]
    public class DungeonConfig : ScriptableObject
    {
        [Header("Identity")]
        public string dungeonId;
        public LocalizedString displayName = new();
        public LocalizedString title = new();
        public LocalizedString description = new();
        public Sprite icon;
        public Sprite previewImage;

        [Header("Levels")]
        public DungeonLevelConfig[] levels;

        public int LevelCount => levels == null ? 0 : levels.Length;
        public int FirstPlayableLevelIndex
        {
            get
            {
                if (levels == null) return -1;

                for (var i = 0; i < levels.Length; i++)
                    if (levels[i] != null && levels[i].IsPlayable)
                        return i;

                return -1;
            }
        }

        public bool HasPlayableLevels
        {
            get => FirstPlayableLevelIndex >= 0;
        }

        public DungeonLevelConfig GetLevel(int index) => levels[index];

        public bool TryGetLevel(int index, out DungeonLevelConfig level)
        {
            if (levels == null || index < 0 || index >= levels.Length)
            {
                level = null;
                return false;
            }

            level = levels[index];
            return level != null;
        }
    }
}
