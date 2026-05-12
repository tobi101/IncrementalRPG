using System;
using System.Collections.Generic;
using System.Linq;
using Core.Save;
using Model;
using Reflex.Attributes;

namespace Core.TestSkillTree
{
    public class SkillTreeService : ISaveable
    {
        [Inject] private SkillTreeConfig _config;
        [Inject] private Player _player;

        private Dictionary<string, NodeDefinition> _nodeMap;
        private SkillTreeState _state;

        private readonly Dictionary<StatType, float> _bonusCache      = new Dictionary<StatType, float>();
        private readonly Dictionary<StatType, float> _multiplierCache = new Dictionary<StatType, float>();
        private readonly HashSet<GameFeature> _unlockedFeatures        = new HashSet<GameFeature>();

        // Fired after any node is successfully upgraded.
        public event Action OnUpgraded;

        public void Load(SaveData data)
        {
            _nodeMap = BuildNodeMap(_config);
            _state   = data.SkillTreeState ?? new SkillTreeState();
            _state.Init();
            RebuildCache();
        }

        public void Contribute(SaveData data)
        {
            data.SkillTreeState = _state;
        }

        public NodeState GetState(string nodeId)
        {
            var def   = GetDefinition(nodeId);
            var level = _state.GetLevel(nodeId);

            if (level >= def.maxLevel)     return NodeState.Complete;
            if (!IsVisible(def))           return NodeState.Hidden;
            if (!ArePrerequisitesMet(def)) return NodeState.Locked;

            var cost = GetUpgradeCost(nodeId);
            return (cost == 0 || _player.GoldTotal >= cost)
                ? NodeState.Affordable
                : NodeState.Unaffordable;
        }

        private bool IsVisible(NodeDefinition def)
        {
            if (def.prerequisites == null || def.prerequisites.Count == 0)
                return true;

            var parent = def.prerequisites[0].node;
            if (parent == null) return true;

            if (_state.GetLevel(parent.id) >= 1) return true;

            if (parent.prerequisites == null || parent.prerequisites.Count == 0)
                return false;

            var grandparent = parent.prerequisites[0].node;
            if (grandparent == null) return false;

            return _state.GetLevel(grandparent.id) >= 1;
        }

        public int GetUpgradeCost(string nodeId)
        {
            var def   = GetDefinition(nodeId);
            var level = _state.GetLevel(nodeId);
            if (level >= def.maxLevel) return 0;
            if (def.goldCostPerLevel == null || level >= def.goldCostPerLevel.Length) return 0;
            return def.goldCostPerLevel[level];
        }

        public bool CanUpgrade(string nodeId)
        {
            var def = GetDefinition(nodeId);
            if (!ArePrerequisitesMet(def) || _state.GetLevel(nodeId) >= def.maxLevel)
                return false;

            var cost = GetUpgradeCost(nodeId);
            return cost == 0 || _player.GoldTotal >= cost;
        }

        public void Upgrade(string nodeId)
        {
            if (!CanUpgrade(nodeId))
                throw new InvalidOperationException($"Cannot upgrade node '{nodeId}'.");

            var cost = GetUpgradeCost(nodeId);
            if (cost > 0)
                _player.GoldTotal -= cost;

            _state.SetLevel(nodeId, _state.GetLevel(nodeId) + 1);
            RebuildCache();
            OnUpgraded?.Invoke();
        }
        
        public int GetLevel(string nodeId) => _state.GetLevel(nodeId);

        public float GetBonus(StatType stat) =>
            _bonusCache.TryGetValue(stat, out var v) ? v : 0f;
        
        public float GetMultiplier(StatType stat) =>
            _multiplierCache.TryGetValue(stat, out var v) ? v : 1f;

        public bool IsUnlocked(GameFeature feature) =>
            _unlockedFeatures.Contains(feature);
        
        private void RebuildCache()
        {
            _bonusCache.Clear();
            _multiplierCache.Clear();
            _unlockedFeatures.Clear();

            foreach (var def in _config.NodeDefinitions)
            {
                var level = _state.GetLevel(def.id);
                if (level == 0) 
                    continue;

                foreach (var effect in def.effects)
                {
                    switch (effect.effectType)
                    {
                        case NodeEffectType.Additive:
                            _bonusCache.TryGetValue(effect.statType, out var bonus);
                            
                            for (var i = 0; i < level && i < effect.valuesPerLevel.Length; i++)
                                bonus += effect.valuesPerLevel[i];
                            
                            _bonusCache[effect.statType] = bonus;
                            break;

                        case NodeEffectType.Multiplicative:
                            _multiplierCache.TryGetValue(effect.statType, out var multSum);
                            
                            for (var i = 0; i < level && i < effect.valuesPerLevel.Length; i++)
                                multSum += effect.valuesPerLevel[i];
                            
                            _multiplierCache[effect.statType] = multSum;
                            break;

                        case NodeEffectType.FeatureUnlock:
                            _unlockedFeatures.Add(effect.feature);
                            break;
                    }
                }
            }
            
            foreach (var key in new List<StatType>(_multiplierCache.Keys))
                _multiplierCache[key] = 1f + _multiplierCache[key];
        }

        private bool ArePrerequisitesMet(NodeDefinition def)
        {
            return def.prerequisites.All(prereq =>
                prereq.node != null &&
                _state.GetLevel(prereq.node.id) >= prereq.requiredLevel);
        }

        private NodeDefinition GetDefinition(string nodeId)
        {
            return _nodeMap.TryGetValue(nodeId, out var def) 
                ? def 
                : throw new ArgumentException($"Node '{nodeId}' not found in SkillTreeConfig.");
        }

        private static Dictionary<string, NodeDefinition> BuildNodeMap(SkillTreeConfig config)
        {
            var map = new Dictionary<string, NodeDefinition>();
            foreach (var node in config.NodeDefinitions)
                map[node.id] = node;
            return map;
        }
    }
    
    public enum NodeState
    {
        Hidden,       // Not visible (grandparent not yet upgraded)
        Locked,       // Visible but direct prerequisite not yet upgraded
        Unaffordable, // Prerequisites met, not enough gold
        Affordable,   // Prerequisites met, enough gold to upgrade
        Complete,     // level == maxLevel
    }
}
