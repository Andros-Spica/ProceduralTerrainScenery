using UnityEngine;

namespace VectorExtension
{
    [System.Serializable]
    public struct Vector2Range
    {
        public Range x, y;

        public Vector2Range(Range x, Range y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 range
        {
            get
            {
                return new Vector2(x.max - x.min, y.max - y.min);
            }
        }

        public override string ToString()
        {
            return string.Format("[Vector2Range]" + x.ToString() + "," + y.ToString());
        }
    }
}
