using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using Utils;

namespace Core.TestSkillTree
{
    public readonly struct SkillNodeRequirement
    {
        public SkillNodeDefinition Node { get; }
        public int Level { get; }

        public SkillNodeRequirement(SkillNodeDefinition node, int level)
        {
            Node = node;
            Level = level;
        }
    }

    // Shared serialized fields retain their names so existing node assets need no migration.
    public abstract class SkillNodeDefinition : ScriptableObject
    {
        public string id;
        public LocalizedString displayName = new();
        public LocalizedString description = new();
        public Sprite icon;
        [Min(1)] public int maxLevel = 1;
        public BigDouble[] goldCostPerLevel = { new BigDouble(100) };

        public virtual int LevelLimit => Mathf.Max(1, maxLevel);
        public abstract IEnumerable<SkillNodeRequirement> Requirements { get; }

        public bool TryGetCost(int level, out BigDouble cost)
        {
            cost = BigDouble.Zero;
            if (level < 0 || level >= LevelLimit || goldCostPerLevel == null || level >= goldCostPerLevel.Length)
                return false;

            var candidate = goldCostPerLevel[level];
            if (!candidate.IsFinite || !candidate.IsNormalized || candidate < 0 ||
                BigDoubleMath.SanitizeNonNegativeInteger(candidate, BigDouble.Zero) != candidate)
                return false;

            cost = candidate;
            return true;
        }
    }
}
