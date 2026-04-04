using System;
using System.Collections.Generic;
using Core.Save;

namespace Core.TestSkillTree
{
    public class SkillTreeService : ISaveable
    {
        private readonly SkillTreeConfig _config;
        private readonly Dictionary<string, NodeDefinition> _nodeMap;
        private SkillTreeState _state;

        private readonly Dictionary<StatType, float> _bonusCache      = new Dictionary<StatType, float>();
        private readonly Dictionary<StatType, float> _multiplierCache = new Dictionary<StatType, float>();
        private readonly HashSet<GameFeature> _unlockedFeatures        = new HashSet<GameFeature>();

        // Fired after any node is successfully upgraded.
        public event Action OnUpgraded;

        public SkillTreeService(SkillTreeConfig config, SaveService saveService)
        {
            _config  = config;
            _nodeMap = BuildNodeMap(config);
            _state   = saveService.GetData().SkillTreeState ?? new SkillTreeState();
            _state.Init();
            RebuildCache();
        }

        public void Load(SaveData data)
        {
            _state = data.SkillTreeState ?? new SkillTreeState();
            _state.Init();
            RebuildCache();
        }

        public void Contribute(SaveData data)
        {
            data.SkillTreeState = _state;
        }

        // ── State ─────────────────────────────────────────────────────────────

        public NodeState GetState(string nodeId)
        {
            var def = GetDefinition(nodeId);
            if (!ArePrerequisitesMet(def)) return NodeState.Hidden;

            int level = _state.GetLevel(nodeId);
            if (level == 0)           return NodeState.Available;
            if (level < def.maxLevel) return NodeState.Partial;
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
            RebuildCache();
            OnUpgraded?.Invoke();
        }

        // ── Stat queries — O(1) via cache ─────────────────────────────────────

        // Sum of all additive bonuses for a stat across the whole tree.
        public float GetBonus(StatType stat) =>
            _bonusCache.TryGetValue(stat, out var v) ? v : 0f;

        // Returns a multiplier: 1.0 = no bonus, 1.25 = +25%, etc.
        public float GetMultiplier(StatType stat) =>
            _multiplierCache.TryGetValue(stat, out var v) ? v : 1f;

        public bool IsUnlocked(GameFeature feature) =>
            _unlockedFeatures.Contains(feature);

        // ── Helpers ───────────────────────────────────────────────────────────

        // Rebuilds stat caches from scratch. Called once on init and after each Upgrade().
        // Upgrades are rare, so a full rebuild is cheaper than partial invalidation.
        private void RebuildCache()
        {
            _bonusCache.Clear();
            _multiplierCache.Clear();
            _unlockedFeatures.Clear();

            foreach (var def in _config.nodes)
            {
                int level = _state.GetLevel(def.id);
                if (level == 0) continue;

                foreach (var effect in def.effects)
                {
                    switch (effect.effectType)
                    {
                        case NodeEffectType.Additive:
                            _bonusCache.TryGetValue(effect.statType, out var bonus);
                            for (int i = 0; i < level && i < effect.valuesPerLevel.Length; i++)
                                bonus += effect.valuesPerLevel[i];
                            _bonusCache[effect.statType] = bonus;
                            break;

                        case NodeEffectType.Multiplicative:
                            _multiplierCache.TryGetValue(effect.statType, out var multSum);
                            for (int i = 0; i < level && i < effect.valuesPerLevel.Length; i++)
                                multSum += effect.valuesPerLevel[i];
                            _multiplierCache[effect.statType] = multSum; // raw sum; +1 applied below
                            break;

                        case NodeEffectType.FeatureUnlock:
                            _unlockedFeatures.Add(effect.feature);
                            break;
                    }
                }
            }

            // Finalise multipliers: stored as raw sum during accumulation, convert to 1+sum
            foreach (var key in new List<StatType>(_multiplierCache.Keys))
                _multiplierCache[key] = 1f + _multiplierCache[key];
        }

        private bool ArePrerequisitesMet(NodeDefinition def)
        {
            foreach (var prereq in def.prerequisites)
                if (_state.GetLevel(prereq.nodeId) < prereq.requiredLevel)
                    return false;
            return true;
        }

        private NodeDefinition GetDefinition(string nodeId)
        {
            if (_nodeMap.TryGetValue(nodeId, out var def)) return def;
            throw new ArgumentException($"Node '{nodeId}' not found in SkillTreeConfig.");
        }

        private static Dictionary<string, NodeDefinition> BuildNodeMap(SkillTreeConfig config)
        {
            var map = new Dictionary<string, NodeDefinition>(config.nodes.Count);
            foreach (var node in config.nodes)
                map[node.id] = node;
            return map;
        }
    }
}
