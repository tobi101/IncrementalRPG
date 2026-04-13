using UnityEngine;

namespace Core.TestSkillTree
{
    [CreateAssetMenu(menuName = "RPG/Skill Tree/Back Glow Sprite Config")]
    public class NodeBackGlowSpriteConfig : ScriptableObject
    {
        public Sprite locked;
        public Sprite unaffordable;
        public Sprite affordable;
        public Sprite complete;

        public Sprite GetSprite(NodeState state) => state switch
        {
            NodeState.Locked       => locked,
            NodeState.Unaffordable => unaffordable,
            NodeState.Affordable   => affordable,
            NodeState.Complete     => complete,
            _                      => null
        };
    }
}
