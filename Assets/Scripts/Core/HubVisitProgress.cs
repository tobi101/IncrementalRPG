using System;
using System.Linq;
using Core.Save;

namespace Core
{
    [Flags]
    public enum HubArea
    {
        None = 0,
        Dungeon = 1 << 0,
        SkillTree = 1 << 1,
        Inventory = 1 << 2,
        Craft = 1 << 3
    }

    public sealed class HubVisitProgress : ISaveable
    {
        private HubArea _visitedAreas;

        public event Action OnChanged;

        public bool HasFinishedFirstRun { get; private set; }

        public bool HasVisited(HubArea area) => (_visitedAreas & area) == area;

        public void MarkFirstRunFinished()
        {
            if (HasFinishedFirstRun)
                return;

            HasFinishedFirstRun = true;
            // Opening training before the first run must not consume its later notification.
            _visitedAreas &= ~HubArea.SkillTree;
            OnChanged?.Invoke();
        }

        public void MarkVisited(HubArea area)
        {
            if (HasVisited(area))
                return;

            _visitedAreas |= area;
            OnChanged?.Invoke();
        }

        public void Contribute(SaveData data)
        {
            data.VisitedHubAreas = _visitedAreas;
            data.HasFinishedFirstRun = HasFinishedFirstRun;
        }

        public void Load(SaveData data)
        {
            _visitedAreas = data.VisitedHubAreas;
            HasFinishedFirstRun = data.HasFinishedFirstRun || data.SavedPlayerInfo.BestSessionKills > 0 ||
                data.SavedPlayerInfo.BestSessionGold > 0 ||
                (data.DungeonProgressState?.entries?.Any(entry => entry != null &&
                    (entry.reachedLevelIndex > 0 || entry.completed)) ?? false);
        }
    }
}
