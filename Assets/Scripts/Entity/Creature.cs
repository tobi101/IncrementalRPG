using System;
using UnityEngine;
using Utils;

namespace Entity
{
    public class Creature : Entity
    {
        public int CurrentHP { get; private set; }
        public bool IsAlive => CurrentHP > 0;

        public event Action<int, int> OnHealthChanged; // current, max
        public event Action<BigDouble> OnDamageTaken;
        public event Action OnDied;

        public Creature(EntityConfig config, Vector2Int tileCoord) : base(config, tileCoord)
        {
            CurrentHP = config.maxHP;
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            var previousHealth = CurrentHP;
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            var damageTaken = Mathf.Max(0, previousHealth - CurrentHP);

            OnHealthChanged?.Invoke(CurrentHP, Config.maxHP);
            if (damageTaken > 0)
                OnDamageTaken?.Invoke(damageTaken);

            if (CurrentHP == 0)
                OnDied?.Invoke();
        }
    }
}
