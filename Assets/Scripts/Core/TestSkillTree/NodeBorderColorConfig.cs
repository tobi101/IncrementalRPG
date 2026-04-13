using UnityEngine;

namespace Core.TestSkillTree
{
    [CreateAssetMenu(menuName = "RPG/Skill Tree/Border Color Config")]
    public class NodeBorderColorConfig : ScriptableObject
    {
        public Color locked;
        public Color unaffordable;
        public Color affordable;
        public Color complete;

        public Color GetColor(NodeState state) => state switch
        {
            NodeState.Locked       => locked,
            NodeState.Unaffordable => unaffordable,
            NodeState.Affordable   => affordable,
            NodeState.Complete     => complete,
            _                      => Color.white
        };
    }
}
