using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace Core.TestSkillTree
{
    [Serializable]
    public class NodePrerequisite
    {
        public NodeDefinition node;
        public int requiredLevel;
    }

    [CreateAssetMenu(fileName = "NodeDefinition", menuName = "RPG/Skill Tree/Node")]
    public class NodeDefinition : ScriptableObject
    {
        [Tooltip("Unique identifier used in code and save data.")]
        public string id;

        public LocalizedString displayName = new();

        [Tooltip("Description shown in the skill tree popup.")]
        public LocalizedString description = new();

        public Sprite icon;

        [Tooltip("Optional icon rendered as a small badge in the node view.")]
        public Sprite additionalIcon;

        [Min(1)]
        public int maxLevel;

        [Tooltip("Gold cost to upgrade to each level. Index 0 = cost for level 0→1, index 1 = cost for 1→2, etc.")]
        public int[] goldCostPerLevel;

        [Tooltip("All prerequisites must be satisfied for this node to become visible.")]
        public List<NodePrerequisite> prerequisites;

        public NodeEffect[] effects;

        [HideInInspector]
        public Vector2 positionInGraph;
    }
}
