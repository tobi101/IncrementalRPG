using System;
using System.Collections.Generic;
using Core.Items;
using UnityEngine;
using Utils;

namespace Core.Gameplay.Dungeon
{
    [Serializable]
    public sealed class WeightedLootEntry
    {
        public ItemDefinition item;
        [Min(0f)] public float weight = 1f;
        [Tooltip("Gold/shard amount for currency rewards. Ignored for inventory items.")]
        public BigDouble currencyAmount = BigDouble.One;

        public bool IsValid => item != null && weight > 0f && !float.IsNaN(weight) && !float.IsInfinity(weight)
            && (item.category != ItemCategory.Currency ||
                (Enum.IsDefined(typeof(RewardCurrency), item.currency) &&
                 BigDoubleMath.SanitizeNonNegativeInteger(currencyAmount, BigDouble.Zero) > 0));
    }

    [Serializable]
    public sealed class LevelLootPool
    {
        public WeightedLootEntry[] entries;

        public List<LootDrop> Roll(int count)
        {
            var result = new List<LootDrop>(Math.Max(0, count));
            var totalWeight = 0d;
            WeightedLootEntry lastValidEntry = null;

            foreach (var entry in entries ?? Array.Empty<WeightedLootEntry>())
                if (entry != null && entry.IsValid)
                {
                    totalWeight += entry.weight;
                    lastValidEntry = entry;
                }

            if (totalWeight <= 0d || double.IsInfinity(totalWeight))
                throw new InvalidOperationException("Loot pool requires a finite, positive total weight and valid rewards.");

            for (var rollIndex = 0; rollIndex < count; rollIndex++)
            {
                var roll = UnityEngine.Random.value * totalWeight;
                var selected = lastValidEntry;

                foreach (var entry in entries)
                {
                    if (entry == null || !entry.IsValid) continue;
                    roll -= entry.weight;
                    if (roll > 0f)
                        continue;

                    selected = entry;
                    break;
                }
                result.Add(new LootDrop(selected.item,
                    BigDoubleMath.SanitizeNonNegativeInteger(selected.currencyAmount, BigDouble.Zero)));
            }

            return result;
        }
    }
}
