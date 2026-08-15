using System;

namespace Utils
{
    public static class BigDoubleMath
    {
        private const long LastExponentWithRepresentableFraction = 15;

        public static BigDouble RoundToInteger(BigDouble value)
        {
            value = value.NormalizedOr(BigDouble.Zero);
            if (value.IsZero || value.Exponent > LastExponentWithRepresentableFraction)
                return value;

            if (value.Exponent < -324)
                return BigDouble.Zero;

            return new BigDouble(Math.Round(value.ToDouble(), 0, MidpointRounding.AwayFromZero));
        }

        public static BigDouble FloorToInteger(BigDouble value)
        {
            value = value.NormalizedOr(BigDouble.Zero);
            if (value.IsZero || value.Exponent > LastExponentWithRepresentableFraction)
                return value;

            if (value.Exponent < -324)
                return value.Mantissa < 0d ? -BigDouble.One : BigDouble.Zero;

            return new BigDouble(Math.Floor(value.ToDouble()));
        }

        public static BigDouble MultiplyAndRound(BigDouble value, double multiplier)
        {
            if (double.IsNaN(multiplier) || double.IsInfinity(multiplier))
                throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be finite.");

            return RoundToInteger(value * multiplier);
        }

        public static BigDouble SanitizeNonNegativeInteger(BigDouble value, BigDouble fallback)
        {
            fallback = fallback.NormalizedOr(BigDouble.Zero);
            if (fallback < BigDouble.Zero)
                fallback = BigDouble.Zero;

            value = value.NormalizedOr(fallback);
            if (value < BigDouble.Zero)
                return fallback;

            return RoundToInteger(value);
        }
    }
}
