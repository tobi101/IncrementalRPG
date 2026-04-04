using System.Collections.Generic;
using UnityEngine;

namespace Core.TestSkillTree
{
    [CreateAssetMenu(fileName = "SkillTreeConfig", menuName = "RPG/Skill Tree/Config")]
    public class SkillTreeConfig : ScriptableObject
    {
        public List<NodeDefinition> nodes;
    }
}
