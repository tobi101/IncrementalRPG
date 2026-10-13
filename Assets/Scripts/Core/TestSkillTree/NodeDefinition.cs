using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Core.TestSkillTree
{
    [Serializable]
    public class NodePrerequisite
    {
        public NodeDefinition node;
        public int requiredLevel;
    }

    [CreateAssetMenu(fileName = "NodeDefinition", menuName = "RPG/Skill Tree/Node")]
    public class NodeDefinition : SkillNodeDefinition
    {
        [Tooltip("Optional icon rendered as a small badge in the node view.")]
        public Sprite additionalIcon;

        [Tooltip("All prerequisites must be satisfied for this node to become visible.")]
        public List<NodePrerequisite> prerequisites;

        public NodeEffect[] effects;

        [HideInInspector]
        public Vector2 positionInGraph;

        public override IEnumerable<SkillNodeRequirement> Requirements
        {
            get
            {
                if (prerequisites == null) yield break;
                foreach (var requirement in prerequisites)
                    yield return new SkillNodeRequirement(requirement?.node, requirement?.requiredLevel ?? 1);
            }
        }

        private void OnValidate()
        {
            if (goldCostPerLevel == null)
                return;

            for (var i = 0; i < goldCostPerLevel.Length; i++)
                goldCostPerLevel[i] = BigDoubleMath.SanitizeNonNegativeInteger(
                    goldCostPerLevel[i], BigDouble.Zero);
        }
    }
}
