namespace VectorExtension
{
    [System.Serializable]
    public struct IntVector2Range
    {
        public IntRange x, y;
        
        public IntVector2Range(IntRange x, IntRange y)
        {
            this.x = x;
            this.y = y;
        }

        public IntVector2 range
        {
            get
            {
                return new IntVector2(x.max - x.min, y.max - y.min);
            }
        }

        public override string ToString()
        {
            return string.Format("[IntVector2Range]" + x.ToString() + "," + y.ToString());
        }
    }
}
