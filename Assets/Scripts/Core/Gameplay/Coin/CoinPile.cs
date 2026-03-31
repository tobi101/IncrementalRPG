using System;
using UnityEngine;

namespace Core.Gameplay
{
    public class CoinPile
    {
        public Vector2Int TileCoord { get; }
        public int Amount { get; private set; }

        public event Action OnChanged;
        public event Action OnCollected;

        public CoinPile(Vector2Int tileCoord, int amount)
        {
            TileCoord = tileCoord;
            Amount = amount;
        }

        public void Add(int amount)
        {
            Amount += amount;
            OnChanged?.Invoke();
        }

        public void Collect()
        {
            OnCollected?.Invoke();
        }
    }
}
