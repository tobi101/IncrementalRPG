using System;
using System.Globalization;

namespace Utils
{
    public static class BigDoubleFormatter
    {
        private static readonly NumberFormatInfo DisplayFormat = NumberFormatInfo.ReadOnly(new NumberFormatInfo
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = ".",
            NumberGroupSizes = new[] { 3 }
        });

        public static string Format(BigDouble value, int decimals = 2) =>
            FormatNumber(value, decimals, false);

        public static string FormatFloor(BigDouble value, int decimals = 2) =>
            FormatNumber(value, decimals, true);

        public static string Format(BigDouble value, int decimalsSmall, int decimalsLarge)
        {
            var decimals = value.Exponent < 3 ? decimalsSmall : decimalsLarge;
            return FormatNumber(value, decimals, false);
        }

        private static string FormatNumber(BigDouble value, int decimals, bool floor)
        {
            value = value.NormalizedOr(BigDouble.Zero);
            if (value.IsZero)
                return "0";

            decimals = Math.Max(0, Math.Min(15, decimals));

            if (value.Exponent < 6)
                return FormatPlain(value, value.Exponent < 0 ? 2 : decimals, floor);

            if (value.Exponent < 12)
                return FormatAbbreviated(value, decimals, floor);

            return FormatEngineering(value, decimals, floor);
        }

        private static string FormatPlain(BigDouble value, int decimals, bool floor)
        {
            // Values this small round to zero at two decimal places, without converting an extreme exponent.
            if (value.Exponent < -3)
                return floor && value.Mantissa < 0d ? "-0,01" : "0";

            var number = Quantize((decimal)value.ToDouble(), decimals, floor);
            if (Math.Abs(number) >= 1000000m)
                return FormatAbbreviated(new BigDouble((double)number), decimals, floor);

            return number == 0m ? "0" : number.ToString("#,0" + FractionFormat(decimals), DisplayFormat);
        }

        private static string FormatAbbreviated(BigDouble value, int decimals, bool floor)
        {
            var unitExponent = value.Exponent < 9 ? 6 : 9;
            var scaled = (decimal)(value.Mantissa * Math.Pow(10d, value.Exponent - unitExponent));
            // M/B show exactly three decimal places, truncated as in the design reference.
            scaled = (floor ? decimal.Floor(scaled * 1000m) : decimal.Truncate(scaled * 1000m)) / 1000m;

            if (Math.Abs(scaled) >= 1000m)
            {
                if (unitExponent == 9)
                    return FormatEngineering(new BigDouble((double)(scaled / 1000m), 12), decimals, floor);

                return (scaled / 1000m).ToString("0.000", DisplayFormat) + "B";
            }

            return scaled.ToString("0.000", DisplayFormat) + (unitExponent == 6 ? "M" : "B");
        }

        private static string FormatEngineering(BigDouble value, int decimals, bool floor)
        {
            var remainder = (int)(value.Exponent % 3);
            var engineeringExponent = value.Exponent - remainder;
            var displayMantissa = Quantize((decimal)(value.Mantissa * Math.Pow(10d, remainder)), decimals, floor);

            if (Math.Abs(displayMantissa) >= 1000m && engineeringExponent <= long.MaxValue - 3)
            {
                displayMantissa /= 1000m;
                engineeringExponent += 3;
            }

            return displayMantissa.ToString("0" + FractionFormat(decimals), DisplayFormat) + "e" +
                engineeringExponent.ToString(CultureInfo.InvariantCulture);
        }

        private static decimal Quantize(decimal value, int decimals, bool floor)
        {
            if (!floor)
                return decimal.Round(value, decimals, MidpointRounding.AwayFromZero);

            var factor = (decimal)Math.Pow(10d, decimals);
            return decimal.Floor(value * factor) / factor;
        }

        private static string FractionFormat(int decimals) => decimals == 0 ? string.Empty : "." + new string('#', decimals);
    }
}
