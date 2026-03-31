using System;

namespace Utils
{
    public static class BigDoubleFormatter
    {
        static readonly string[] NamedSuffixes =
        {
            "",     // 10^0
            "K",    // 10^3
            "M",    // 10^6
            "B",    // 10^9
            "T",    // 10^12
            "Qa",   // 10^15
            "Qi",   // 10^18
            "Sx",   // 10^21
            "Sp",   // 10^24
            "Oc",   // 10^27
            "No",   // 10^30
            "De",   // 10^33
        };

        // Generates: aa, ab, ... az, ba, bb, ... zz, aaa, ...
        static string GenerateSuffix(long index)
        {
            long i = index;
            string suffix = "";
            do
            {
                suffix = (char)('a' + i % 26) + suffix;
                i = i / 26 - 1;
            }
            while (i >= 0);

            return suffix;
        }

        static string GetSuffix(long exponent)
        {
            if (exponent < 0) return "";

            long index = exponent / 3;

            if (index < NamedSuffixes.Length)
                return NamedSuffixes[index];

            return GenerateSuffix(index - NamedSuffixes.Length);
        }

        /// <summary>
        /// Formats a BigDouble for display.
        /// Small numbers (below 1000) show no suffix and no decimals.
        /// Larger numbers show 2 decimal places and a suffix (K, M, B, ... aa, ab, ...).
        /// </summary>
        public static string Format(BigDouble value, int decimals = 2)
        {
            if (value.Mantissa == 0) return "0";

            long exp = value.Exponent;

            // Align exponent to nearest lower multiple of 3
            int mod = (int)((exp % 3 + 3) % 3);
            double displayMantissa = value.Mantissa * Math.Pow(10, mod);
            long displayExp = exp - mod;

            string suffix = GetSuffix(displayExp);

            // Numbers below 1000: no suffix, no decimals
            if (displayExp < 3)
                return ((long)Math.Round(displayMantissa * Math.Pow(10, mod - mod))).ToString();

            string format = decimals > 0 ? $"F{decimals}" : "F0";
            return $"{displayMantissa.ToString(format)}{suffix}";
        }

        /// <summary>
        /// Format with explicit decimal control for both small and large numbers.
        /// </summary>
        public static string Format(BigDouble value, int decimalsSmall, int decimalsLarge)
        {
            if (value.Mantissa == 0) return "0";

            long exp = value.Exponent;
            int mod = (int)((exp % 3 + 3) % 3);
            double displayMantissa = value.Mantissa * Math.Pow(10, mod);
            long displayExp = exp - mod;

            string suffix = GetSuffix(displayExp);

            if (displayExp < 3)
            {
                string smallFmt = decimalsSmall > 0 ? $"F{decimalsSmall}" : "F0";
                return displayMantissa.ToString(smallFmt);
            }

            string largeFmt = decimalsLarge > 0 ? $"F{decimalsLarge}" : "F0";
            return $"{displayMantissa.ToString(largeFmt)}{suffix}";
        }
    }
}
