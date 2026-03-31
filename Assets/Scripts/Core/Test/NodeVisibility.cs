namespace Core.Test
{
    public enum NodeVisibility
    {
        Hidden,     // prerequisites не выполнены — узел не отображается
        Unlocked,   // виден, 0 / N уровней
        Partial,    // виден, M / N уровней (0 < M < N)
        Full        // виден, N / N уровней
    }
}
