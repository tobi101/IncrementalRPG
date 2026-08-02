using System.Collections.Generic;
using UnityEngine;

namespace Core.Gameplay.Shards
{
    public static class ShardDropMath
    {
        public static List<int> BuildPickupValues(int baseDrop, int nominalPickupValue, int finalDrop)
        {
            var result = new List<int>();
            baseDrop = Mathf.Max(0, baseDrop);
            finalDrop = Mathf.Max(0, finalDrop);
            nominalPickupValue = Mathf.Max(1, nominalPickupValue);

            if (baseDrop == 0 || finalDrop == 0)
                return result;

            var allocatedBase = 0;
            var allocatedFinal = 0;

            while (allocatedBase < baseDrop)
            {
                var baseValue = Mathf.Min(nominalPickupValue, baseDrop - allocatedBase);
                allocatedBase += baseValue;

                var targetAllocated = allocatedBase == baseDrop
                    ? finalDrop
                    : Mathf.RoundToInt((float)finalDrop * allocatedBase / baseDrop);
                var pickupValue = targetAllocated - allocatedFinal;
                allocatedFinal = targetAllocated;

                if (pickupValue > 0)
                    result.Add(pickupValue);
            }

            return result;
        }
    }
}
