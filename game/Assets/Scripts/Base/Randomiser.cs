using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Base
{
    /// <summary>
    /// Initializes a single Random object for the whole program, in order to overcome flaws in Random implementation.
    /// </summary>
    public static class Randomiser
    {
        private static readonly Random s_staticRandom = new Random();

        public static int Next()
        {
            return s_staticRandom.Next();
        }

        public static int Next(int maxValue)
        {
            return s_staticRandom.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return s_staticRandom.Next(minValue, maxValue);
        }

        public static double NextDouble()
        {
            return s_staticRandom.NextDouble();
        }

        public static double NextDouble(double min, double max)
        {
            return min + s_staticRandom.NextDouble() * (max - min);
        }

        //See if random sample comes lower than the given chance
        public static bool ProbabilityCheck(double chance)
        {
            Assert.EqualOrLesser(chance, 1, "we can't have a probablity higher than 1");
            return (NextDouble() <= chance);
        }

        //choose a single value out of a collection
        public static T ChooseValue<T>(IEnumerable<T> group)
        {
            Assert.NotNullOrEmpty(group,"group");
            T current = default(T);
            int count = 0;
            foreach (T element in group)
            {
                count++;
                if (s_staticRandom.Next(count) == 0)
                {
                    current = element;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
        }

        //choose several values out of a collection
        public static IEnumerable<T> ChooseValues<T>(IEnumerable<T> group, int amount)
        {
            return Shuffle(group).Take(amount);
        }

        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> group)
        {
            Assert.NotNullOrEmpty(group, "group");
            var buffer = group.ToList();

            for (int i = 0; i < buffer.Count; i++)
            {
                int j = s_staticRandom.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

        internal static bool CoinToss()
        {
            return Next(2) > 0;
        }
    }
}