using UnityEngine;
using System.Collections;

namespace Utilities {

    public static class Function
    {
        public static float DiminishingReturns(float input, float mostEfficientInput, float maxOutcome)
        {
            return input * maxOutcome / (input + mostEfficientInput);
        }

        //public static float LogisticIntegral(float input, float max, float rate, float midValue = 0.5f)
        //{
        //    return max / (1 + Mathf.Exp(- rate * (input - max * midValue)));
        //}

        public static float LogisticDerivate(float value, float max, float rate)
        {
            return rate * value * (1 - value / max);
        }

    }
	
}
