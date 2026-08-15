using System;
using System.Globalization;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// A base-10 floating-point value with a 64-bit exponent.
    ///
    /// The representation follows the mantissa/exponent approach also used by
    /// BreakInfinity.cs, but is intentionally kept small and owned by this project.
    /// </summary>
    [Serializable]
    public struct BigDouble : IComparable<BigDouble>, IEquatable<BigDouble>
    {
        private const int AdditionPrecisionDigits = 17;

        [SerializeField] private double mantissa;
        [SerializeField] private long exponent;

        public double Mantissa => mantissa;
        public long Exponent => exponent;
        public bool IsFinite => !double.IsNaN(mantissa) && !double.IsInfinity(mantissa);
        public bool IsZero => mantissa == 0d;
        public bool IsNormalized
        {
            get
            {
                if (!IsFinite)
                    return false;

                if (mantissa == 0d)
                    return exponent == 0;

                var absoluteMantissa = Math.Abs(mantissa);
                return absoluteMantissa >= 1d && absoluteMantissa < 10d;
            }
        }

        public static readonly BigDouble Zero = new BigDouble(0d, 0);
        public static readonly BigDouble One = new BigDouble(1d, 0);

        public BigDouble(double mantissa, long exponent)
        {
            Normalize(mantissa, exponent, out this.mantissa, out this.exponent);
        }

        public BigDouble(double value) : this(value, 0) { }

        public BigDouble(long value) : this((double)value, 0) { }

        public BigDouble NormalizedOr(BigDouble fallback)
        {
            if (!IsFinite)
                return fallback.IsNormalized ? fallback : Zero;

            try
            {
                return IsNormalized ? this : new BigDouble(mantissa, exponent);
            }
            catch (OverflowException)
            {
                return fallback.IsNormalized ? fallback : Zero;
            }
        }

        public double ToDouble()
        {
            var value = RequireCanonical(this);

            if (value.IsZero)
                return 0d;

            if (value.exponent > 308)
                return value.mantissa > 0d ? double.PositiveInfinity : double.NegativeInfinity;

            if (value.exponent < -324)
                return value.mantissa > 0d ? 0d : -0d;

            if (value.exponent == -324)
                return value.mantissa * 1e-308 * 1e-16;

            return value.mantissa * Math.Pow(10d, value.exponent);
        }

        public string ToScientificString()
        {
            var value = RequireCanonical(this);
            if (value.IsZero)
                return "0";

            return value.mantissa.ToString("G17", CultureInfo.InvariantCulture)
                   + "e"
                   + value.exponent.ToString(CultureInfo.InvariantCulture);
        }

        public static bool TryParse(string text, out BigDouble value)
        {
            value = Zero;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();
            var exponentSeparator = text.LastIndexOfAny(new[] { 'e', 'E' });
            var mantissaText = exponentSeparator >= 0 ? text.Substring(0, exponentSeparator) : text;
            var exponentText = exponentSeparator >= 0 ? text.Substring(exponentSeparator + 1) : null;

            if (!double.TryParse(mantissaText, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedMantissa)
                || double.IsNaN(parsedMantissa)
                || double.IsInfinity(parsedMantissa))
                return false;

            var parsedExponent = 0L;
            if (exponentText != null
                && !long.TryParse(exponentText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
                    out parsedExponent))
                return false;

            try
            {
                value = new BigDouble(parsedMantissa, parsedExponent);
                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        public static BigDouble Abs(BigDouble value)
        {
            value = RequireCanonical(value);
            return value.mantissa < 0d ? new BigDouble(-value.mantissa, value.exponent) : value;
        }

        public static BigDouble Min(BigDouble left, BigDouble right) => left <= right ? left : right;

        public static BigDouble Max(BigDouble left, BigDouble right) => left >= right ? left : right;

        public static BigDouble operator +(BigDouble left, BigDouble right)
        {
            left = RequireCanonical(left);
            right = RequireCanonical(right);

            if (left.IsZero)
                return right;
            if (right.IsZero)
                return left;

            if (left.exponent >= right.exponent)
            {
                var difference = ExponentDifference(left.exponent, right.exponent);
                if (difference > AdditionPrecisionDigits)
                    return left;

                return new BigDouble(
                    left.mantissa + right.mantissa * Math.Pow(10d, -(int)difference),
                    left.exponent);
            }

            var reverseDifference = ExponentDifference(right.exponent, left.exponent);
            if (reverseDifference > AdditionPrecisionDigits)
                return right;

            return new BigDouble(
                right.mantissa + left.mantissa * Math.Pow(10d, -(int)reverseDifference),
                right.exponent);
        }

        public static BigDouble operator -(BigDouble left, BigDouble right) => left + -right;

        public static BigDouble operator -(BigDouble value)
        {
            value = RequireCanonical(value);
            return value.IsZero ? Zero : new BigDouble(-value.mantissa, value.exponent);
        }

        public static BigDouble operator *(BigDouble left, BigDouble right)
        {
            left = RequireCanonical(left);
            right = RequireCanonical(right);

            if (left.IsZero || right.IsZero)
                return Zero;

            return new BigDouble(left.mantissa * right.mantissa,
                CheckedAddExponents(left.exponent, right.exponent));
        }

        public static BigDouble operator /(BigDouble left, BigDouble right)
        {
            left = RequireCanonical(left);
            right = RequireCanonical(right);

            if (right.IsZero)
                throw new DivideByZeroException();
            if (left.IsZero)
                return Zero;

            return new BigDouble(left.mantissa / right.mantissa,
                CheckedSubtractExponents(left.exponent, right.exponent));
        }

        public static BigDouble operator +(BigDouble left, double right) => left + new BigDouble(right);
        public static BigDouble operator -(BigDouble left, double right) => left - new BigDouble(right);
        public static BigDouble operator *(BigDouble left, double right) => left * new BigDouble(right);
        public static BigDouble operator /(BigDouble left, double right) => left / new BigDouble(right);

        public static bool operator ==(BigDouble left, BigDouble right) => left.Equals(right);
        public static bool operator !=(BigDouble left, BigDouble right) => !left.Equals(right);
        public static bool operator >(BigDouble left, BigDouble right) => left.CompareTo(right) > 0;
        public static bool operator <(BigDouble left, BigDouble right) => left.CompareTo(right) < 0;
        public static bool operator >=(BigDouble left, BigDouble right) => left.CompareTo(right) >= 0;
        public static bool operator <=(BigDouble left, BigDouble right) => left.CompareTo(right) <= 0;

        public static implicit operator BigDouble(double value) => new BigDouble(value);
        public static implicit operator BigDouble(long value) => new BigDouble(value);
        public static implicit operator BigDouble(int value) => new BigDouble(value);
        public static explicit operator double(BigDouble value) => value.ToDouble();
        public static explicit operator float(BigDouble value) => (float)value.ToDouble();

        public int CompareTo(BigDouble other)
        {
            var left = RequireCanonical(this);
            var right = RequireCanonical(other);

            if (left.mantissa == right.mantissa && left.exponent == right.exponent)
                return 0;

            var leftSign = Math.Sign(left.mantissa);
            var rightSign = Math.Sign(right.mantissa);
            if (leftSign != rightSign)
                return leftSign.CompareTo(rightSign);

            if (left.exponent != right.exponent)
            {
                var exponentComparison = left.exponent.CompareTo(right.exponent);
                return leftSign > 0 ? exponentComparison : -exponentComparison;
            }

            return left.mantissa.CompareTo(right.mantissa);
        }

        public bool Equals(BigDouble other)
        {
            if (!IsFinite || !other.IsFinite)
                return false;

            var left = RequireCanonical(this);
            var right = RequireCanonical(other);
            return left.mantissa.Equals(right.mantissa) && left.exponent == right.exponent;
        }

        public override bool Equals(object obj) => obj is BigDouble other && Equals(other);

        public override int GetHashCode()
        {
            if (!IsFinite)
                return 0;

            var value = RequireCanonical(this);
            return HashCode.Combine(value.mantissa, value.exponent);
        }

        public override string ToString() => BigDoubleFormatter.Format(this);

        private static BigDouble RequireCanonical(BigDouble value)
        {
            if (!value.IsFinite)
                throw new ArithmeticException("BigDouble cannot contain NaN or Infinity.");

            return value.IsNormalized ? value : new BigDouble(value.mantissa, value.exponent);
        }

        private static void Normalize(double sourceMantissa, long sourceExponent,
            out double normalizedMantissa, out long normalizedExponent)
        {
            if (double.IsNaN(sourceMantissa) || double.IsInfinity(sourceMantissa))
                throw new ArgumentOutOfRangeException(nameof(sourceMantissa), "Mantissa must be finite.");

            if (sourceMantissa == 0d)
            {
                normalizedMantissa = 0d;
                normalizedExponent = 0;
                return;
            }

            var workingMantissa = sourceMantissa;
            var workingExponent = sourceExponent;
            var absoluteMantissa = Math.Abs(workingMantissa);

            // Bring the value into a range where Math.Pow(10, adjustment) cannot underflow.
            if (absoluteMantissa >= 1e308)
            {
                workingMantissa /= 1e308;
                workingExponent = CheckedAddExponents(workingExponent, 308);
            }
            else if (absoluteMantissa < 1e-307)
            {
                workingMantissa *= 1e308;
                workingExponent = CheckedAddExponents(workingExponent, -308);
            }

            var adjustment = (long)Math.Floor(Math.Log10(Math.Abs(workingMantissa)));
            workingMantissa /= Math.Pow(10d, adjustment);
            workingExponent = CheckedAddExponents(workingExponent, adjustment);

            // Correct the occasional boundary drift caused by binary floating point.
            absoluteMantissa = Math.Abs(workingMantissa);
            if (absoluteMantissa >= 10d)
            {
                workingMantissa /= 10d;
                workingExponent = CheckedAddExponents(workingExponent, 1);
            }
            else if (absoluteMantissa < 1d)
            {
                workingMantissa *= 10d;
                workingExponent = CheckedAddExponents(workingExponent, -1);
            }

            normalizedMantissa = workingMantissa;
            normalizedExponent = workingExponent;
        }

        private static ulong ExponentDifference(long larger, long smaller) =>
            unchecked((ulong)larger - (ulong)smaller);

        private static long CheckedAddExponents(long left, long right)
        {
            checked
            {
                return left + right;
            }
        }

        private static long CheckedSubtractExponents(long left, long right)
        {
            checked
            {
                return left - right;
            }
        }
    }
}
