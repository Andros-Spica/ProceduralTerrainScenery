using System;
using UnityEngine;

namespace VectorExtension
{
    [System.Serializable]
    public struct IntVector2
    {
        public int x, y;

        public static readonly IntVector2 zero = new IntVector2(0, 0);
        public static readonly IntVector2 one = new IntVector2(1, 1);

        public static readonly IntVector2 right = new IntVector2(1, 0);
        public static readonly IntVector2 left = new IntVector2(-1, 0);

        public static readonly IntVector2 up = new IntVector2(0, 1);
        public static readonly IntVector2 down = new IntVector2(0, -1);

        public IntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public float magnitude
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y);
            }
        }

        public Vector2 Normalize()
        {
            return new Vector2(x / magnitude, y / magnitude);
        }

        public static Vector2 Normalize(IntVector2 a)
        {
            float magnitude = a.magnitude;
            return new Vector2(a.x / magnitude, a.y / magnitude);
        }

        public override bool Equals(object other)
        {
            if (!(other is IntVector2))
            {
                return false;
            }
            IntVector2 v = (IntVector2)other;
            return x == v.x &&
                y == v.y;
        }

        public override string ToString()
        {
            return string.Format("[IntVector2]" + x + "," + y);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2;
        }

        public float DistanceSquare(IntVector2 v)
        {
            return IntVector2.DistanceSquare(this, v);
        }
        public static float DistanceSquare(IntVector2 a, IntVector2 b)
        {
            float cx = b.x - a.x;
            float cy = b.y - a.y;
            return cx * cx + cy * cy;
        }

        public static bool operator ==(IntVector2 a, IntVector2 b)
        {
            return a.x == b.x &&
                a.y == b.y;
        }

        public static bool operator !=(IntVector2 a, IntVector2 b)
        {
            return a.x != b.x ||
                a.y != b.y;
        }

        public static IntVector2 operator -(IntVector2 a, IntVector2 b)
        {
            return new IntVector2(a.x - b.x, a.y - b.y);
        }

        public static IntVector2 operator +(IntVector2 a, IntVector2 b)
        {
            return new IntVector2(a.x + b.x, a.y + b.y);
        }

        public static IntVector2 operator *(IntVector2 a, int i)
        {
            return new IntVector2((int)a.x * i, (int)a.y * i);
        }

        public static IntVector2 Min(IntVector2 a, IntVector2 b)
        {
            return new IntVector2((int)Math.Min(a.x, b.x), (int)Math.Min(a.y, b.y));
        }
        public static IntVector2 Max(IntVector2 a, IntVector2 b)
        {
            return new IntVector2(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
        }
    }
}
