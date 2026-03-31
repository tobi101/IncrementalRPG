using System.Collections.Generic;

namespace Core.Test
{
    public class SkillTreeModel
    {
        private readonly SkillTreeConfig _config;
        private readonly Dictionary<string, int> _levels = new();

        public IReadOnlyDictionary<string, int> NodeLevels => _levels;

        public SkillTreeModel(SkillTreeConfig config)
        {
            _config = config;
        }

        public int GetLevel(string nodeId) =>
            _levels.TryGetValue(nodeId, out var l) ? l : 0;

        public NodeVisibility GetVisibility(SkillNodeConfig node)
        {
            if (!ArePrerequisitesMet(node)) return NodeVisibility.Hidden;
            int lvl = GetLevel(node.Id);
            if (lvl == 0) return NodeVisibility.Unlocked;
            if (lvl < node.MaxLevel) return NodeVisibility.Partial;
            return NodeVisibility.Full;
        }

        public bool CanUpgrade(SkillNodeConfig node, int gold) =>
            GetVisibility(node) != NodeVisibility.Hidden &&
            GetLevel(node.Id) < node.MaxLevel &&
            gold >= node.GoldCostPerLevel;

        public void Upgrade(string nodeId)
        {
            _levels[nodeId] = GetLevel(nodeId) + 1;
        }

        public void LoadFrom(List<NodeSaveEntry> entries)
        {
            _levels.Clear();
            foreach (var entry in entries)
                _levels[entry.NodeId] = entry.Level;
        }

        public List<NodeSaveEntry> ToSaveEntries()
        {
            var result = new List<NodeSaveEntry>();
            foreach (var (id, level) in _levels)
                result.Add(new NodeSaveEntry { NodeId = id, Level = level });
            return result;
        }

        private bool ArePrerequisitesMet(SkillNodeConfig node)
        {
            foreach (var prereq in node.Prerequisites)
                if (GetLevel(prereq.Id) < node.PrerequisiteMinLevel)
                    return false;
            return true;
        }
    }
}
