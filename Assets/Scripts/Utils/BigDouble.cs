using System;

namespace Utils
{
    [Serializable]
    public struct BigDouble : IComparable<BigDouble>, IEquatable<BigDouble>
    {
        public double Mantissa;
        public long Exponent;

        public static readonly BigDouble Zero = new BigDouble(0, 0);
        public static readonly BigDouble One = new BigDouble(1.0, 0);

        public BigDouble(double mantissa, long exponent)
        {
            Mantissa = mantissa;
            Exponent = exponent;
            Normalize(ref this);
        }

        public BigDouble(double value)
        {
            if (value == 0.0)
            {
                Mantissa = 0;
                Exponent = 0;
                return;
            }

            Exponent = (long)Math.Floor(Math.Log10(Math.Abs(value)));
            Mantissa = value / Math.Pow(10, Exponent);
            Normalize(ref this);
        }

        public BigDouble(long value) : this((double)value) { }

        static void Normalize(ref BigDouble n)
        {
            if (n.Mantissa == 0.0)
            {
                n.Exponent = 0;
                return;
            }

            while (Math.Abs(n.Mantissa) >= 10.0) { n.Mantissa /= 10.0; n.Exponent++; }
            while (Math.Abs(n.Mantissa) < 1.0)   { n.Mantissa *= 10.0; n.Exponent--; }
        }

        // Arithmetic

        public static BigDouble operator +(BigDouble a, BigDouble b)
        {
            if (a.Mantissa == 0) return b;
            if (b.Mantissa == 0) return a;

            if (a.Exponent > b.Exponent + 17) return a;
            if (b.Exponent > a.Exponent + 17) return b;

            long diff = a.Exponent - b.Exponent;
            double m = a.Mantissa * Math.Pow(10, diff) + b.Mantissa;
            return new BigDouble(m, b.Exponent);
        }

        public static BigDouble operator -(BigDouble a, BigDouble b)
        {
            return a + new BigDouble(-b.Mantissa, b.Exponent);
        }

        public static BigDouble operator *(BigDouble a, BigDouble b)
        {
            return new BigDouble(a.Mantissa * b.Mantissa, a.Exponent + b.Exponent);
        }

        public static BigDouble operator /(BigDouble a, BigDouble b)
        {
            if (b.Mantissa == 0) throw new DivideByZeroException();
            return new BigDouble(a.Mantissa / b.Mantissa, a.Exponent - b.Exponent);
        }

        public static BigDouble operator +(BigDouble a, double b) => a + new BigDouble(b);
        public static BigDouble operator -(BigDouble a, double b) => a - new BigDouble(b);
        public static BigDouble operator *(BigDouble a, double b) => new BigDouble(a.Mantissa * b, a.Exponent);
        public static BigDouble operator /(BigDouble a, double b) => new BigDouble(a.Mantissa / b, a.Exponent);

        // Comparison

        public static bool operator ==(BigDouble a, BigDouble b) => a.Exponent == b.Exponent && Math.Abs(a.Mantissa - b.Mantissa) < 1e-10;
        public static bool operator !=(BigDouble a, BigDouble b) => !(a == b);

        public static bool operator >(BigDouble a, BigDouble b)
        {
            if (a.Exponent != b.Exponent) return a.Exponent > b.Exponent;
            return a.Mantissa > b.Mantissa;
        }

        public static bool operator <(BigDouble a, BigDouble b) => b > a;
        public static bool operator >=(BigDouble a, BigDouble b) => !(a < b);
        public static bool operator <=(BigDouble a, BigDouble b) => !(a > b);

        // Implicit conversions

        public static implicit operator BigDouble(double value) => new BigDouble(value);
        public static implicit operator BigDouble(long value)   => new BigDouble(value);
        public static implicit operator BigDouble(int value)    => new BigDouble((double)value);

        public static explicit operator double(BigDouble value)
            => value.Mantissa * Math.Pow(10, value.Exponent);

        public static explicit operator float(BigDouble value)
            => (float)(value.Mantissa * Math.Pow(10, value.Exponent));

        // IComparable / IEquatable

        public int CompareTo(BigDouble other)
        {
            if (this > other) return 1;
            if (this < other) return -1;
            return 0;
        }

        public bool Equals(BigDouble other) => this == other;

        public override bool Equals(object obj) => obj is BigDouble other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Mantissa, Exponent);

        public override string ToString() => BigDoubleFormatter.Format(this);
    }
}
