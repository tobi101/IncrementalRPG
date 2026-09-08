using System;
using UnityEngine;
using Utils;

namespace Entity
{
    public class Creature : Entity
    {
        public BigDouble CurrentHP { get; private set; }
        public bool IsAlive => CurrentHP > 0;
        public Vector3 WorldPosition { get; internal set; }
        public bool IsMoving { get; internal set; }

        public event Action<BigDouble, BigDouble> OnHealthChanged; // current, max
        public event Action<BigDouble> OnDamageTaken;
        public event Action OnDied;

        public Creature(EntityConfig config, Vector2Int tileCoord) : base(config, tileCoord)
        {
            CurrentHP = config.maxHP;
        }

        internal void SetTilePosition(Vector2Int tileCoord, Vector3 worldPosition)
        {
            TileCoord = tileCoord;
            WorldPosition = worldPosition;
        }

        public void TakeDamage(BigDouble amount)
        {
            if (!IsAlive) return;

            amount = BigDoubleMath.SanitizeNonNegativeInteger(amount, BigDouble.Zero);
            if (amount <= BigDouble.Zero) return;

            var previousHealth = CurrentHP;
            CurrentHP = BigDouble.Max(BigDouble.Zero, CurrentHP - amount);
            var damageTaken = BigDouble.Max(BigDouble.Zero, previousHealth - CurrentHP);

            OnHealthChanged?.Invoke(CurrentHP, Config.maxHP);
            if (damageTaken > 0)
                OnDamageTaken?.Invoke(damageTaken);

            if (CurrentHP == BigDouble.Zero)
                OnDied?.Invoke();
        }
    }
}
