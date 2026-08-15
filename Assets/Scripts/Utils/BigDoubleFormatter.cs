using System;
using System.Globalization;

namespace Utils
{
    public static class BigDoubleFormatter
    {
        public static string Format(BigDouble value, int decimals = 2) =>
            FormatEngineering(value, decimals, false);

        public static string FormatFloor(BigDouble value, int decimals = 2) =>
            FormatEngineering(value, decimals, true);

        public static string Format(BigDouble value, int decimalsSmall, int decimalsLarge)
        {
            var decimals = value.Exponent < 3 ? decimalsSmall : decimalsLarge;
            return FormatEngineering(value, decimals, false);
        }

        private static string FormatEngineering(BigDouble value, int decimals, bool floor)
        {
            value = value.NormalizedOr(BigDouble.Zero);
            if (value.IsZero)
                return "0";

            decimals = Math.Max(0, Math.Min(15, decimals));

            // long.MinValue cannot be rounded down to the preceding multiple of three.
            if (value.Exponent < long.MinValue + 2)
                return value.ToScientificString();

            var remainder = (int)((value.Exponent % 3 + 3) % 3);
            var engineeringExponent = value.Exponent - remainder;
            var displayMantissa = value.Mantissa * Math.Pow(10d, remainder);
            var factor = Math.Pow(10d, decimals);

            displayMantissa = floor
                ? Math.Floor(displayMantissa * factor) / factor
                : Math.Round(displayMantissa, decimals, MidpointRounding.AwayFromZero);

            if (!floor && Math.Abs(displayMantissa) >= 1000d && engineeringExponent <= long.MaxValue - 3)
            {
                displayMantissa /= 1000d;
                engineeringExponent += 3;
            }

            var format = decimals == 0 ? "0" : "0." + new string('#', decimals);
            var mantissaText = displayMantissa.ToString(format, CultureInfo.InvariantCulture);
            return engineeringExponent == 0
                ? mantissaText
                : mantissaText + "e" + engineeringExponent.ToString(CultureInfo.InvariantCulture);
        }
    }
}
