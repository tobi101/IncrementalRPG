using System;
using System.Collections.Generic;
using Model;
using Utils;

namespace Core.TestSkillTree
{
    // Save models implement this contract without changing their existing JSON layout.
    public interface ISkillTreeLevels
    {
        int GetLevel(string nodeId);
        void SetLevel(string nodeId, int level);
    }

    public enum SkillPurchaseStatus { Available, RequirementsMissing, NotEnoughGold, Complete, NotConfigured }

    // One progression engine for Skills and each class. Presentation and rewards live outside it.
    public sealed class SkillTreeProgression
    {
        private readonly HashSet<SkillNodeDefinition> _nodes;
        private readonly HashSet<string> _duplicateIds = new();
        private readonly ISkillTreeLevels _levels;
        private readonly Player _player;

        public bool IsPurchasing { get; private set; }

        public SkillTreeProgression(IEnumerable<SkillNodeDefinition> nodes, ISkillTreeLevels levels, Player player)
        {
            _nodes = new HashSet<SkillNodeDefinition>(nodes ?? Array.Empty<SkillNodeDefinition>());
            _levels = levels ?? throw new ArgumentNullException(nameof(levels));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            var ids = new HashSet<string>();
            foreach (var node in _nodes)
                if (node != null && !ids.Add(node.id)) _duplicateIds.Add(node.id);
        }

        public int GetLevel(SkillNodeDefinition node) => Contains(node) ? _levels.GetLevel(node.id) : 0;

        public SkillPurchaseStatus GetStatus(SkillNodeDefinition node)
        {
            if (!Contains(node)) return SkillPurchaseStatus.NotConfigured;
            var level = GetLevel(node);
            if (level >= node.LevelLimit) return SkillPurchaseStatus.Complete;
            foreach (var requirement in node.Requirements)
                if (!Contains(requirement.Node) || GetLevel(requirement.Node) < Math.Max(1, requirement.Level))
                    return SkillPurchaseStatus.RequirementsMissing;
            if (!node.TryGetCost(level, out var cost)) return SkillPurchaseStatus.NotConfigured;
            return _player.GoldTotal >= cost ? SkillPurchaseStatus.Available : SkillPurchaseStatus.NotEnoughGold;
        }

        // Rewards must be prepared before calling. The callback records spend / rebuilds effects
        // after the level changes and before gold observers see the completed purchase.
        public bool TryPurchase(SkillNodeDefinition node, Action<BigDouble> onLevelPurchased = null)
        {
            if (IsPurchasing || GetStatus(node) != SkillPurchaseStatus.Available) return false;
            var level = GetLevel(node);
            node.TryGetCost(level, out var cost);
            IsPurchasing = true;
            try
            {
                _levels.SetLevel(node.id, level + 1);
                onLevelPurchased?.Invoke(cost);
                if (cost > BigDouble.Zero) _player.GoldTotal -= cost;
            }
            finally
            {
                IsPurchasing = false;
            }
            return true;
        }

        private bool Contains(SkillNodeDefinition node) => node != null && !string.IsNullOrEmpty(node.id) &&
            !_duplicateIds.Contains(node.id) && _nodes.Contains(node);
    }
}
