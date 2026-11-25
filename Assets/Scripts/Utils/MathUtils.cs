using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shared
{
    public static class MathUtils
    {

        public static bool Encapsulates(this Bounds big, Bounds small)
        {
            // Check if small is inside big (all min and max components of small bounds are within big bounds)
            return small.min.x >= big.min.x &&
                   small.min.y >= big.min.y &&
                   small.max.x <= big.max.x &&
                   small.max.y <= big.max.y;
        }

        public static float GeneralizedSigmoid(float x, float a = -0.02f,float k = 1.02f, float b = 9, float q = 1, float c = 1)
        {
            return a + ((k - a) / (c + (q * Mathf.Exp(-b * (x - 0.5f)))));
        }

        // Integrate function f(x) using the trapezoidal rule between x=x_low..x_high
        public static float Integrate(AnimationCurve f, float x_low = 0, float x_high = 1, int N_steps = 100)
        {
            float h = (x_high - x_low) / N_steps;
            float res = (f.Evaluate(x_low) + f.Evaluate(x_high)) / 2;
            for (int i = 1; i < N_steps; i++) {
                res += f.Evaluate(x_low + i * h);
            }
            return h * res;
        }

        /// <summary>
        /// Round a float number to a specific number of decimals
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static float Round(float number, int decimals)
        {
            float mult = Mathf.Pow(10.0f, (float)decimals);
            return (Mathf.Round(number * mult)) / mult;
        }

        /// <summary>
        /// Round a float number with specific step size 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="stepSize"></param>
        /// <returns></returns>
        public static float RoundStep(float number, float stepSize)
        {
            return Mathf.Round(number / stepSize) * stepSize;
        }

        /// <summary>
        /// Round a float number with specific step size 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="stepSize"></param>
        /// <returns></returns>
        public static Vector2 RoundStep(Vector2 vec, float stepSize)
        {
            return new Vector2(RoundStep(vec.x, stepSize), RoundStep(vec.y, stepSize));
        }

        /// <summary>
        /// Check if two numbers have the same sign, by using the custom <see cref="GenericUtils.Sign"/> function
        /// </summary>
        /// <param name="numberA"></param>
        /// <param name="numberB"></param>
        /// <returns></returns>
        public static bool SameSign(float numberA, float numberB)
        {
            return Sign(numberA) == Sign(numberB);
        }


        public static int SampleIndexFromDistribution(IEnumerable<float> distribution)
        {
            float totalWeight = 0;
            foreach (float prob in distribution)
                totalWeight += prob;

            float r = Random.Range(0, totalWeight);

            totalWeight = 0;
            for (int i = 0; i < distribution.Count(); i++) {
                totalWeight += distribution.ElementAt(i);
                if (r < totalWeight)
                    return i;
            }

            return distribution.Count() - 1;
        }

        public static IEnumerable<T> SampleRandomly<T>(IEnumerable<T> items, int nurofSamples)
        {
            nurofSamples = Mathf.Clamp(nurofSamples, 0, items.Count());
            IEnumerable<T> shuffledItems = GenericUtils.Shuffle(items).ToList();
            return shuffledItems.Take(nurofSamples);
        }

        /// <summary>
        /// Custom sign function that also returns 0 if the number is exactly 0.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static float Sign(float number)
        {
            return number < 0 ? -1 : (number > 0 ? 1 : 0);
        }

    }
}
