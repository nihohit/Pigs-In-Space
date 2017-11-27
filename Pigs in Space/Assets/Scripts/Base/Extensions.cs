﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Base
{
    public interface IIdentifiable<out T>
    {
        T Name { get; }
    }

    /// <summary>
    /// extensions of basic C# objects
    /// </summary>
    public static class MyExtensions
    {
        public static bool ProbabilityCheck(this double chance)
        {
            return Randomiser.ProbabilityCheck(chance);
        }

        public static T SafeCast<T>(this object obj, string name) where T : class
        {
            var result = obj as T;

            Assert.NotNull(result, name, "Tried to cast {0} to {1}".FormatWith(obj, typeof(T)), 3);

            return result;
        }

        public static string FormatWith(this string str, params object[] formattingInfo)
        {
            return string.Format(str, formattingInfo);
        }

        // try to get a value out of a dictionary, and if it doesn't exist, create it by a given method
        public static TValue TryGetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> itemCreationMethod)
        {
            TValue result;
            if (dict.TryGetValue(key, out result))
            {
                return result;
            }

            result = itemCreationMethod();
            dict.Add(key, result);

            return result;
        }

        // removes from both sets the common elements.
        public static void ExceptOnBoth<T>(this HashSet<T> thisSet, HashSet<T> otherSet)
        {
            thisSet.SymmetricExceptWith(otherSet);
            otherSet.IntersectWith(thisSet);
            thisSet.ExceptWith(otherSet);
        }

        // converts degrees to radians
        public static float DegreesToRadians(this float degrees)
        {
            return (float)Math.PI * degrees / 180;
        }

        public static bool HasFlag(this Enum value, Enum flag)
        {
            return (Convert.ToInt64(value) & Convert.ToInt64(flag)) > 0;
        }

        #region IEnumerable

        public static IEnumerable<T> Duplicate<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Select(item => item).Materialize();
        }

        // returns an enumerable with all values of an enumerator
        public static IEnumerable<T> GetValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> group)
        {
            Assert.NotNull(group, "group");
            return Randomiser.Shuffle(group);
        }

        // this function ensures that a given enumeration materializes
        public static IEnumerable<T> Materialize<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is ICollection<T>)
            {
                return enumerable;
            }

            return enumerable.ToList();
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> op)
        {
            if (enumerable == null)
            {
                return;
            }

            foreach (var val in enumerable)
            {
                op(val);
            }
        }

        public static bool None<T>(this IEnumerable<T> enumerable, Func<T, bool> op)
        {
            Assert.NotNull(enumerable, "enumerable");
            return !enumerable.Any(op);
        }

        public static bool None<T>(this IEnumerable<T> enumerable)
        {
            Assert.NotNull(enumerable, "enumerable");
            return !enumerable.Any();
        }

        public static T ChooseRandomValue<T>(this IEnumerable<T> group)
        {
            Assert.NotNull(group, "group");
            return Randomiser.ChooseValue(group);
        }

        public static IEnumerable<T> ChooseRandomValues<T>(this IEnumerable<T> group, int amount)
        {
            Assert.NotNull(group, "group");
            return Randomiser.ChooseValues(group, amount);
        }

        // Converts an IEnumerator to IEnumerable
        public static IEnumerable<object> ToEnumerable(this IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        // Join two enumerators into a new one
        public static IEnumerator Join(this IEnumerator enumerator, IEnumerator other)
        {
            if (other != null)
            {
                return enumerator.ToEnumerable().Union(other.ToEnumerable()).GetEnumerator();
            }

            return enumerator;
        }

        public static string ToJoinedString<T>(this IEnumerable<T> enumerable, string separator)
        {
            return string.Join(separator, enumerable.Select(item => item.ToString()).ToArray());
        }

        #endregion IEnumerable

        #region dictionaries

        public static IEnumerable<T> ChooseWeightedValues<T>(this IDictionary<T, double> dictionary, int amount)
        {
            return Randomiser.ChooseWeightedValues(dictionary, amount);
        }

        //try to get a value out of a dictionary, and if it doesn't exist, create it by a given method
        public static T TryGetOrAdd<T, S>(this IDictionary<S, T> dict, S key, Func<T> itemCreationMethod)
        {
            T result;
            if (!dict.TryGetValue(key, out result))
            {
                result = itemCreationMethod();
                dict.Add(key, result);
            }
            return result;
        }

        //try to get a value out of a dictionary, and if it doesn't exist, enter a default value
        public static T TryGetOrAdd<T, S>(this IDictionary<S, T> dict, S key, T value)
        {
            T result;
            if (!dict.TryGetValue(key, out result))
            {
                result = value;
                dict.Add(key, result);
            }
            return result;
        }

        //try to get a value out of a dictionary, and if it doesn't exist, enter a default value
        public static T TryGetOrDefaultValue<T, S>(this IDictionary<S, T> dict, S key, T value)
        {
            T result;
            if (!dict.TryGetValue(key, out result))
            {
                return value;
            }
            return result;
        }

        public static TVal Get<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key, string dictionaryName = "dictionary")
        {
            TVal value;
            if (!dict.TryGetValue(key, out value))
            {
                Assert.AssertConditionMet(dict.ContainsKey(key), "Key \'{0}\' not found in {1}".FormatWith(key, dictionaryName));
            }
            return value;
        }

        #endregion

        #region 2d arrays

        public static IEnumerable<T> ToEnumerable<T>(this T[,] array)
        {
            foreach (var value in array)
            {
                yield return value;
            }
        }

        public static IEnumerable<T> GetNeighbours<T>(this T[,] array, int x, int y, bool diagonals)
        {
            List<T> neighbours = new List<T>();

            if (x > 0) neighbours.Add(array[x - 1, y]);
            if (x < array.GetLength(0) - 1) neighbours.Add(array[x + 1, y]);
            if (y > 0) neighbours.Add(array[x, y - 1]);
            if (y < array.GetLength(1) - 1) neighbours.Add(array[x, y + 1]);
            if (diagonals)
            {
                if ((x > 0) && (y > 0)) neighbours.Add(array[x - 1, y - 1]);
                if ((x < array.GetLength(0) - 1) && (y > 0)) neighbours.Add(array[x + 1, y - 1]);
                if ((x > 0) && (y < array.GetLength(1))) neighbours.Add(array[x - 1, y + 1]);
                if ((x < array.GetLength(0) - 1) && (y < array.GetLength(1))) neighbours.Add(array[x + 1, y + 1]);
            }

            return neighbours;
        }

        #endregion 2d arrays
    }

    /// <summary>
    /// allows classes to have simple hashing, by sending a list of defining factor to the hasher.
    /// Notice that for good hashing, all values must be from immutable fields.
    /// </summary>
    public static class Hasher
    {
        private const int c_initialHash = 53; // Prime number
        private const int c_multiplier = 29; // Different prime number

        public static int GetHashCode(params object[] values)
        {
            unchecked
            {
                // Overflow is fine, just wrap
                int hash = c_initialHash;

                if (values != null)
                {
                    hash = values.Aggregate(
                        hash,
                        (current, currentObject) =>
                            (current * c_multiplier) + (currentObject != null ? currentObject.GetHashCode() : 0));
                }

                return hash;
            }
        }
    }

    public class EmptyEnumerator : IEnumerator
    {
        public object Current
        {
            get { throw new UnreachableCodeException(); }
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }
    }
}