using UnityEngine;

namespace VectorExtension
{
    [System.Serializable]
    public struct Distribution
    {
        public float mean, stdDev;
        public Range range;

        public Distribution(float mean = 0f, float stdDev = 1f, float min = -5f, float max = 5f)
        {
            this.mean = mean;
            this.stdDev = stdDev;
            this.range.min = (min > mean - stdDev) ? mean - stdDev : min;
            this.range.max = (max < mean + stdDev) ? mean + stdDev : max;
        }
    }
}
