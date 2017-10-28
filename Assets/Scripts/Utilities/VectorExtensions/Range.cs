using System;

namespace VectorExtension
{
    [System.Serializable]
    public struct Range
    {
        public float min, max;

        public Range(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float range
        {
            get
            {
                return max - min;
            }
        }

        public override bool Equals(object other)
        {
            if (!(other is Range))
            {
                return false;
            }
            Range v = (Range)other;
            return min == v.min &&
                max == v.max;
        }

        public override string ToString()
        {
            return string.Format("[Range]" + min + "," + max);
        }

        public override int GetHashCode()
        {
            return min.GetHashCode() ^ max.GetHashCode() << 2;
        }

        public static bool operator ==(Range a, Range b)
        {
            return a.min == b.min &&
                a.max == b.max;
        }

        public static bool operator !=(Range a, Range b)
        {
            return a.min != b.min ||
                a.max != b.max;
        }

        public static Range operator *(Range a, float n)
        {
            return new Range(a.min * n, a.max * n);
        }

        public static Range operator /(Range a, float n)
        {
            return new Range(a.min / n, a.max / n);
        }

        public static Range Min(Range a, Range b)
        {
            return new Range(Math.Min(a.min, b.min), Math.Min(a.max, b.max));
        }
        public static Range Max(Range a, Range b)
        {
            return new Range(Math.Max(a.min, b.min), Math.Max(a.max, b.max));
        }
    }
}
