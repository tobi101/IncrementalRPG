using System;
using UnityEngine;

namespace Entity
{
    public class Creature : Entity
    {
        public int CurrentHP { get; private set; }

        public event Action<int, int> OnHealthChanged; // current, max
        public event Action OnDied;

        public Creature(EntityConfig config, Vector2Int tileCoord) : base(config, tileCoord)
        {
            CurrentHP = config.maxHP;
        }

        public void TakeDamage(int amount)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            OnHealthChanged?.Invoke(CurrentHP, Config.maxHP);

            if (CurrentHP == 0)
                OnDied?.Invoke();
        }
    }
}
