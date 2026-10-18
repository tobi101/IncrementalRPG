using Utils;

namespace Core.Gameplay.Dungeon
{
    internal sealed class LevelExperienceProgress
    {
        public BigDouble Current { get; private set; } = BigDouble.Zero;
        public BigDouble Goal { get; private set; } = BigDouble.Zero;
        public bool IsGoalReached => Goal > BigDouble.Zero && Current >= Goal;

        public void Reset(BigDouble goal)
        {
            Current = BigDouble.Zero;
            Goal = BigDoubleMath.SanitizeNonNegativeInteger(goal, BigDouble.Zero);
        }

        public void Add(BigDouble amount)
        {
            amount = BigDoubleMath.SanitizeNonNegativeInteger(amount, BigDouble.Zero);
            if (amount <= BigDouble.Zero)
                return;

            Current = BigDoubleMath.RoundToInteger(Current + amount);
        }
    }
}
