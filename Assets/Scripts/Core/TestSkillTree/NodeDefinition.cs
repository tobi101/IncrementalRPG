using System;
using System.Collections.Generic;
using UnityEngine;

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

        [Min(1)]
        public int maxLevel;

        [Tooltip("All prerequisites must be satisfied for this node to become visible.")]
        public List<NodePrerequisite> prerequisites;

        public NodeEffect[] effects;

        [Tooltip("Position in the skill tree UI graph.")]
        public Vector2 positionInGraph;
    }
}
