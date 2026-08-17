using System;
using System.Collections.Generic;
using Core.Items;
using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    [Serializable]
    public sealed class WeightedLootEntry
    {
        public ItemDefinition item;
        [Min(0f)] public float weight = 1f;
    }

    [Serializable]
    public sealed class LevelLootPool
    {
        public WeightedLootEntry[] entries;

        public List<ItemDefinition> Roll(int count)
        {
            var result = new List<ItemDefinition>(count);
            var totalWeight = 0f;

            foreach (var entry in entries)
                totalWeight += entry.weight;

            for (var rollIndex = 0; rollIndex < count; rollIndex++)
            {
                var roll = UnityEngine.Random.value * totalWeight;

                foreach (var entry in entries)
                {
                    roll -= entry.weight;
                    if (roll > 0f)
                        continue;

                    result.Add(entry.item);
                    break;
                }
            }

            return result;
        }
    }
}
