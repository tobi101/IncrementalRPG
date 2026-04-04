namespace Core.TestSkillTree
{
    public enum NodeState
    {
        Hidden,    // Prerequisites not met — node is not visible
        Available, // Prerequisites met, level = 0
        Partial,   // 0 < level < maxLevel
        Complete,  // level == maxLevel
    }
}
