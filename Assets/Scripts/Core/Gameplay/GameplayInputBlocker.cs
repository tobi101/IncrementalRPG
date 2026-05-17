using System;

namespace Core.Gameplay
{
    public sealed class GameplayInputBlocker
    {
        public bool IsBlocked { get; private set; }

        public event Action<bool> OnChanged;

        public void SetBlocked(bool blocked)
        {
            if (IsBlocked == blocked)
                return;

            IsBlocked = blocked;
            OnChanged?.Invoke(IsBlocked);
        }
    }
}
