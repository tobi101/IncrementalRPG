namespace Core.TestSkillTree
{
    public enum StatType
    {
        ZoneRadius,
        ZoneDamage,
        SpawnSpeed,
        SpawnCountMax,
    }

    public enum GameFeature
    {
        Bombs,
    }

    public enum NodeEffectType
    {
        Additive,
        Multiplicative,
        FeatureUnlock,
    }

    public enum NodeState
    {
        Hidden,    // Prerequisites not met — node is not visible
        Available, // Prerequisites met, level = 0
        Partial,   // 0 < level < maxLevel
        Complete,  // level == maxLevel
    }
}
