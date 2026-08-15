using System;
using System.Collections.Generic;
using Utils;

namespace Core.Gameplay.Shards
{
    public static class ShardDropMath
    {
        public static List<BigDouble> BuildPickupValues(BigDouble baseDrop, BigDouble nominalPickupValue,
            BigDouble finalDrop, int maxPickupCount)
        {
            var result = new List<BigDouble>();
            baseDrop = BigDoubleMath.SanitizeNonNegativeInteger(baseDrop, BigDouble.Zero);
            finalDrop = BigDoubleMath.SanitizeNonNegativeInteger(finalDrop, BigDouble.Zero);
            nominalPickupValue = BigDoubleMath.SanitizeNonNegativeInteger(nominalPickupValue, BigDouble.One);

            if (baseDrop <= BigDouble.Zero || finalDrop <= BigDouble.Zero)
                return result;

            if (nominalPickupValue < BigDouble.One)
                nominalPickupValue = BigDouble.One;

            maxPickupCount = Math.Max(1, maxPickupCount);
            var desiredCount = baseDrop / nominalPickupValue;
            var pickupCount = desiredCount >= maxPickupCount
                ? maxPickupCount
                : Math.Max(1, (int)Math.Ceiling(desiredCount.ToDouble()));

            if (finalDrop < pickupCount)
                pickupCount = Math.Max(1, (int)finalDrop.ToDouble());

            var remaining = finalDrop;
            for (var i = 0; i < pickupCount; i++)
            {
                var pickupsLeft = pickupCount - i;
                var pickupValue = pickupsLeft == 1
                    ? remaining
                    : BigDoubleMath.FloorToInteger(remaining / pickupsLeft);

                if (pickupValue <= BigDouble.Zero)
                    continue;

                result.Add(pickupValue);
                remaining -= pickupValue;
            }

            return result;
        }
    }
}
