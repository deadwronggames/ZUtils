using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsRandom
    {
        private const bool USE_CONST_SEED = false; // set to true for debugging
        private static readonly System.Random s_seedGenerator = new();
        private static readonly System.Random s_random = GetSystemRandom();
        
        public static System.Random GetSystemRandom()
        {
#pragma warning disable CS0162 // Code is heuristically unreachable
            return new System.Random((USE_CONST_SEED) ? 42 : s_seedGenerator.Next());
#pragma warning restore CS0162 // Code is heuristically unreachable
        }

        public static float RandomizeByPercent(this float averageValue, float percent01Based, bool doClampMinTo0 = true, bool doClampMax100Percent = false)
        {
            float minValue = averageValue * (1 - percent01Based);
            float maxValue = averageValue * (1 + percent01Based);

            if (doClampMinTo0) minValue = Mathf.Max(0f, minValue);
            if (doClampMax100Percent) maxValue = Mathf.Min(maxValue, 2*averageValue);
            
            return (float)s_random.NextDouble() * (maxValue - minValue) + minValue;
        }

        public static TEntry GetRandomEntry<TEntry>(this IEnumerable<TEntry> enumerable) => GetRandomEntry(enumerable, out _);
        public static TEntry GetRandomEntry<TEntry>(this IEnumerable<TEntry> enumerable, out int index)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable), "Collection cannot be null.");
        
            // convert to IList to avoid multiple enumerations and optimize for arrays
            IList<TEntry> entries = enumerable as IList<TEntry> ?? enumerable.ToList();
        
            if (entries.Count == 0) throw new InvalidOperationException($"{nameof(ZMethods)}.{nameof(GetRandomEntry)}: Cannot select a random entry from an empty collection.");

            index = s_random.Next(entries.Count);
            return entries[index];
        }
        
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection) // TODO test after refactoring
        {
            List<T> list = collection.ToList();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = s_random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }

            return list;
        }

        public static List<int> GetShuffledIndices(int size)
        {
            List<int> list = Enumerable.Range(0, size).ToList();
            Shuffle(list);
            return list;
        }
        
        public static bool CoinFlip() => (s_random.Next(2) == 0);
        public static bool Chance(float probability) => (s_random.NextDouble() < probability);
        
        /// <summary>
        /// Returns a random index based on the given indexWeights.
        /// </summary>
        /// <param name="indexWeights">An array of indexWeights. Don't need to be normalized.</param>
        /// <returns>An index corresponding to the selected probability.</returns>
        public static int GetRandomIndexByWeight(float[] indexWeights)
        {
            if (indexWeights == null || indexWeights.Length == 0)
                throw new ArgumentException("Probabilities array must not be null or empty.");

            float[] weightsModified = new float[indexWeights.Length];
            
            // normalize
            float sumOfWeights = indexWeights.Sum();
            if (!sumOfWeights.IsGreaterEqualThanFloat(0f)) 
                throw new ArgumentException("Sum of weights is zero.");
            for (int i = 0; i < indexWeights.Length; i++)
                weightsModified[i] = indexWeights[i] / sumOfWeights;
            
            // calculate cumulative indexWeights
            for (int i = 1; i < indexWeights.Length; i++)
                weightsModified[i] = weightsModified[i - 1] + weightsModified[i];
            
            // determine and return the index based on the random value and cumulative indexWeights
            float randomValue = (float)s_random.NextDouble();
            for (int i = 0; i < weightsModified.Length; i++)
                if (randomValue.IsLesserEqualThanFloat(weightsModified[i])) return i;

            // Fallback - should not reach here
            throw new InvalidOperationException("Unable to determine index from indexWeights.");
        }
        
        public static float GaussRandom(float mean, float stdDev)
        {
            // use Box-Muller transform to generate a standard normal variable
            double u1 = s_random.NextDouble();
            double u2 = s_random.NextDouble();
            float z0 = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));

            // scale and shift to get normal distribution
            return z0 * stdDev + mean;
        }

        public static float LogNormalRandom(float meanParameter, float stdDevParameter) => Mathf.Exp(GaussRandom(meanParameter, stdDevParameter)); // PDF: f(x) = 1 / (x * b * sqrt(2pi)) * e^(-(ln(x)-a)^2) / (2b^2))

        public static Vector2 RandomOnUnitCircle()
        {
            double angle = (2f * Math.PI * s_random.NextDouble());
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
    }
}