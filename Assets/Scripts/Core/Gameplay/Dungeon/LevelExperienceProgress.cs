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
            Goal = BigDouble.Max(BigDouble.Zero, goal.NormalizedOr(BigDouble.Zero));
        }

        public void Add(BigDouble amount)
        {
            amount = amount.NormalizedOr(BigDouble.Zero);
            if (amount <= BigDouble.Zero)
                return;

            Current += amount;
        }
    }
}
