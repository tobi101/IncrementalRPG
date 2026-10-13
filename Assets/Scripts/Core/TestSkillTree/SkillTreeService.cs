using System;
using System.Collections.Generic;
using Core.Save;
using Model;
using Reflex.Attributes;
using Utils;

namespace Core.TestSkillTree
{
    public class SkillTreeService : ISaveable
    {
        [Inject] private SkillTreeConfig _config;
        [Inject] private Player _player;

        private Dictionary<string, NodeDefinition> _nodeMap;
        private SkillTreeState _state;
        private SkillTreeProgression _progression;

        public bool IsPurchasing => _progression?.IsPurchasing == true;

        private readonly Dictionary<StatType, float> _bonusCache      = new Dictionary<StatType, float>();
        private readonly Dictionary<StatType, float> _multiplierCache = new Dictionary<StatType, float>();
        private readonly HashSet<GameFeature> _unlockedFeatures        = new HashSet<GameFeature>();

        // Fired after any node is successfully upgraded.
        public event Action OnUpgraded;
        // Fired after a specific node is successfully upgraded.
        public event Action<string> OnNodeUpgraded;

        public void Load(SaveData data)
        {
            _nodeMap = BuildNodeMap(_config);
            _state   = data.SkillTreeState ?? new SkillTreeState();
            _state.Init();
            _progression = new SkillTreeProgression(_config.NodeDefinitions, _state, _player);
            RebuildCache();
        }

        public void Contribute(SaveData data)
        {
            data.SkillTreeState = _state;
        }

        public NodeState GetState(string nodeId)
        {
            var def   = GetDefinition(nodeId);
            var status = _progression.GetStatus(def);
            if (status == SkillPurchaseStatus.Complete) return NodeState.Complete;
            if (!IsVisible(def)) return NodeState.Hidden;
            return status switch
            {
                SkillPurchaseStatus.Available => NodeState.Affordable,
                SkillPurchaseStatus.NotEnoughGold => NodeState.Unaffordable,
                _ => NodeState.Locked
            };
        }

        private bool IsVisible(NodeDefinition def)
        {
            if (def.prerequisites == null || def.prerequisites.Count == 0)
                return true;

            var parent = def.prerequisites[0]?.node;
            if (parent == null) return true;

            if (_state.GetLevel(parent.id) >= 1) return true;

            if (parent.prerequisites == null || parent.prerequisites.Count == 0)
                return false;

            var grandparent = parent.prerequisites[0]?.node;
            if (grandparent == null) return false;

            return _state.GetLevel(grandparent.id) >= 1;
        }

        public BigDouble GetUpgradeCost(string nodeId)
        {
            var def   = GetDefinition(nodeId);
            return def.TryGetCost(_progression.GetLevel(def), out var cost) ? cost : BigDouble.Zero;
        }

        public bool CanUpgrade(string nodeId)
        {
            return _progression.GetStatus(GetDefinition(nodeId)) == SkillPurchaseStatus.Available;
        }

        public void Upgrade(string nodeId)
        {
            if (TryUpgrade(nodeId) == NodeUpgradeResult.Failed)
                throw new InvalidOperationException($"Cannot upgrade node '{nodeId}'.");
        }

        public NodeUpgradeResult TryUpgrade(string nodeId)
        {
            var def = GetDefinition(nodeId);
            if (!_progression.TryPurchase(def, _ => RebuildCache()))
                return NodeUpgradeResult.Failed;

            var newLevel = _progression.GetLevel(def);
            OnUpgraded?.Invoke();
            OnNodeUpgraded?.Invoke(nodeId);

            return newLevel >= def.LevelLimit
                ? NodeUpgradeResult.UpgradedToMax
                : NodeUpgradeResult.Upgraded;
        }
        
        public int GetLevel(string nodeId) => _progression.GetLevel(GetDefinition(nodeId));

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

                foreach (var effect in def.effects ?? Array.Empty<NodeEffect>())
                {
                    if (effect == null) continue;
                    switch (effect.effectType)
                    {
                        case NodeEffectType.Additive:
                            _bonusCache.TryGetValue(effect.statType, out var bonus);
                            
                            for (var i = 0; i < level && i < (effect.valuesPerLevel?.Length ?? 0); i++)
                                bonus += effect.valuesPerLevel[i];
                            
                            _bonusCache[effect.statType] = bonus;
                            break;

                        case NodeEffectType.Multiplicative:
                            _multiplierCache.TryGetValue(effect.statType, out var multSum);
                            
                            for (var i = 0; i < level && i < (effect.valuesPerLevel?.Length ?? 0); i++)
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

    public enum NodeUpgradeResult
    {
        Failed,
        Upgraded,
        UpgradedToMax,
    }
}
