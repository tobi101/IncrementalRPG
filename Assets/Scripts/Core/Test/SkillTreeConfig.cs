using UnityEngine;

namespace Core.Test
{
    [CreateAssetMenu(fileName = "SkillTree", menuName = "SkillTree/Tree")]
    public class SkillTreeConfig : ScriptableObject
    {
        public SkillNodeConfig[] AllNodes;
    }
}
