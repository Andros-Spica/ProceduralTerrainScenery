using System;

namespace VectorExtension
{
    [System.Serializable]
    public struct IntRange
    {
        public int min, max;

        public IntRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public int range
        {
            get
            {
                return max - min;
            }
        }

        public override bool Equals(object other)
        {
            if (!(other is IntRange))
            {
                return false;
            }
            IntRange v = (IntRange)other;
            return min == v.min &&
                max == v.max;
        }

        public static IntRange operator *(IntRange a, int n)
        {
            return new IntRange(a.min * n, a.max * n);
        }

        public override string ToString()
        {
            return string.Format("[IntRange]" + min + "," + max);
        }

        public override int GetHashCode()
        {
            return min.GetHashCode() ^ max.GetHashCode() << 2;
        }

        public static bool operator ==(IntRange a, IntRange b)
        {
            return a.min == b.min &&
                a.max == b.max;
        }

        public static bool operator !=(IntRange a, IntRange b)
        {
            return a.min != b.min ||
                a.max != b.max;
        }

        public static IntRange Min(IntRange a, IntRange b)
        {
            return new IntRange((int)Math.Min(a.min, b.min), (int)Math.Min(a.max, b.max));
        }
        public static IntRange Max(IntRange a, IntRange b)
        {
            return new IntRange(Math.Max(a.min, b.min), Math.Max(a.max, b.max));
        }
    }
}
