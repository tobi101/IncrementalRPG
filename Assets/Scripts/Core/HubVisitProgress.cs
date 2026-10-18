using System;
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

        public bool HasVisited(HubArea area) => (_visitedAreas & area) == area;

        public void MarkVisited(HubArea area)
        {
            if (HasVisited(area))
                return;

            _visitedAreas |= area;
            OnChanged?.Invoke();
        }

        public void Contribute(SaveData data) => data.VisitedHubAreas = _visitedAreas;

        public void Load(SaveData data) => _visitedAreas = data.VisitedHubAreas;
    }
}
