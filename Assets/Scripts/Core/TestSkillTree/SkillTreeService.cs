using System;

namespace Core.TestSkillTree
{
    public class SkillTreeService
    {
        private readonly SkillTreeConfig _config;
        private readonly SkillTreeState _state;

        // Fired after any node is successfully upgraded.
        public event Action OnUpgraded;

        public SkillTreeService(SkillTreeConfig config, SkillTreeState state)
        {
            _config = config;
            _state = state;
            _state.Init();
        }

        // ── State ─────────────────────────────────────────────────────────────

        public NodeState GetState(string nodeId)
        {
            var def = GetDefinition(nodeId);
            if (!ArePrerequisitesMet(def)) return NodeState.Hidden;

            int level = _state.GetLevel(nodeId);
            if (level == 0)              return NodeState.Available;
            if (level < def.maxLevel)    return NodeState.Partial;
            return NodeState.Complete;
        }

        public bool CanUpgrade(string nodeId)
        {
            var def = GetDefinition(nodeId);
            return ArePrerequisitesMet(def) && _state.GetLevel(nodeId) < def.maxLevel;
        }

        public void Upgrade(string nodeId)
        {
            if (!CanUpgrade(nodeId))
                throw new InvalidOperationException($"Cannot upgrade node '{nodeId}'.");

            _state.SetLevel(nodeId, _state.GetLevel(nodeId) + 1);
            OnUpgraded?.Invoke();
        }

        // ── Stat queries ──────────────────────────────────────────────────────

        // Sum of all additive bonuses for a stat across the whole tree.
        public float GetBonus(StatType stat)
        {
            float total = 0f;
            foreach (var def in _config.nodes)
            {
                int level = _state.GetLevel(def.id);
                if (level == 0) continue;
                foreach (var effect in def.effects)
                {
                    if (effect.effectType != NodeEffectType.Additive) continue;
                    if (effect.statType != stat) continue;
                    for (int i = 0; i < level && i < effect.valuesPerLevel.Length; i++)
                        total += effect.valuesPerLevel[i];
                }
            }
            return total;
        }

        // Returns a multiplier: 1.0 = no bonus, 1.25 = +25%, etc.
        public float GetMultiplier(StatType stat)
        {
            float sum = 0f;
            foreach (var def in _config.nodes)
            {
                int level = _state.GetLevel(def.id);
                if (level == 0) continue;
                foreach (var effect in def.effects)
                {
                    if (effect.effectType != NodeEffectType.Multiplicative) continue;
                    if (effect.statType != stat) continue;
                    for (int i = 0; i < level && i < effect.valuesPerLevel.Length; i++)
                        sum += effect.valuesPerLevel[i];
                }
            }
            return 1f + sum;
        }

        public bool IsUnlocked(GameFeature feature)
        {
            foreach (var def in _config.nodes)
            {
                if (_state.GetLevel(def.id) == 0) continue;
                foreach (var effect in def.effects)
                    if (effect.effectType == NodeEffectType.FeatureUnlock && effect.feature == feature)
                        return true;
            }
            return false;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private bool ArePrerequisitesMet(NodeDefinition def)
        {
            foreach (var prereq in def.prerequisites)
                if (_state.GetLevel(prereq.nodeId) < prereq.requiredLevel)
                    return false;
            return true;
        }

        private NodeDefinition GetDefinition(string nodeId)
        {
            foreach (var def in _config.nodes)
                if (def.id == nodeId) return def;
            throw new ArgumentException($"Node '{nodeId}' not found in SkillTreeConfig.");
        }
    }
}
