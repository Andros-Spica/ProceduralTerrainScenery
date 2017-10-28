using System;
using UnityEngine;

namespace VectorExtension
{

    [System.Serializable]
    public struct IntVector3
    {
        public int x, y, z;

        public static readonly IntVector3 zero = new IntVector3(0, 0, 0);
        public static readonly IntVector3 one = new IntVector3(1, 1, 1);

        public static readonly IntVector3 right = new IntVector3(1, 0, 0);
        public static readonly IntVector3 left = new IntVector3(-1, 0, 0);

        public static readonly IntVector3 up = new IntVector3(0, 1, 0);
        public static readonly IntVector3 down = new IntVector3(0, -1, 0);

        public static readonly IntVector3 forward = new IntVector3(0, 0, 1);
        public static readonly IntVector3 backward = new IntVector3(0, 0, -1);

        public IntVector3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float magnitude
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }
        }

        public Vector3 Normalize()
        {
            return new Vector3(x / magnitude, y / magnitude, z / magnitude);
        }

        public static Vector3 Normalize(IntVector3 a)
        {
            float magnitude = a.magnitude;
            return new Vector3(a.x / magnitude, a.y / magnitude, a.z / magnitude);
        }

        public override bool Equals(object other)
        {
            if (!(other is IntVector3))
            {
                return false;
            }
            IntVector3 v = (IntVector3)other;
            return x == v.x &&
                y == v.y &&
                z == v.z;
        }

        public override string ToString()
        {
            return string.Format("[IntVector3]" + x + "," + y);
        }

        public override int GetHashCode()
        {
            return unchecked(x + (31 * y) + (31 * 31 * z));
        }

        public float DistanceSquare(IntVector3 v)
        {
            return IntVector3.DistanceSquare(this, v);
        }
        public static float DistanceSquare(IntVector3 a, IntVector3 b)
        {
            float cx = b.x - a.x;
            float cy = b.y - a.y;
            float cz = b.z - a.z;
            return cx * cx + cy * cy + cz * cz;
        }

        public static bool operator ==(IntVector3 a, IntVector3 b)
        {
            return a.x == b.x &&
                a.y == b.y &&
                a.z == b.z;
        }

        public static bool operator !=(IntVector3 a, IntVector3 b)
        {
            return a.x != b.x ||
                a.y != b.y ||
                a.z != b.z;
        }

        public static IntVector3 operator -(IntVector3 a, IntVector3 b)
        {
            return new IntVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static IntVector3 operator +(IntVector3 a, IntVector3 b)
        {
            return new IntVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static IntVector3 operator *(IntVector3 a, int i)
        {
            return new IntVector3((int)a.x * i, (int)a.y * i, (int)a.z * i);
        }

        public static IntVector3 Min(IntVector3 a, IntVector3 b)
        {
            return new IntVector3((int)Math.Min(a.x, b.x), (int)Math.Min(a.y, b.y), (int)Math.Min(a.z, b.z));
        }
        public static IntVector3 Max(IntVector3 a, IntVector3 b)
        {
            return new IntVector3(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
        }
    }
}
