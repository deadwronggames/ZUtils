using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethods
    {
        public static Action EmptyAction { get; } = () => { };

        public static T LazyInitialization<T>(ref T backingField, Func<T> initializationFunc) where T : class
        {
            return backingField ??= initializationFunc.Invoke();
        }

        public static bool AllNotNull(this IEnumerable<object> objects) => objects.All(obj => obj != null);
        public static bool IsDefaultValue<T>(T value)         { return EqualityComparer<T>.Default.Equals(value, default); }
        public static bool IsSameValue<T>(T value1, T value2) { return EqualityComparer<T>.Default.Equals(value1, value2); }
        public static bool IsSameFloatValue(float value1, float value2) => Math.Abs(value1 - value2) <= GetFloatTolerance(value1, value2);
        public static bool IsLesserEqualThanFloat(this float value1, float value2) => value1 < value2 + GetFloatTolerance(value1, value2);
        public static bool IsGreaterEqualThanFloat(this float value1, float value2) => value1 > value2 - GetFloatTolerance(value1, value2);
        private static float GetFloatTolerance(float value1, float value2)
        {
            // issues with float comparison:
            // - false positive: values are theoretically different but classified as equal
            // - false negative: values are theoretically equal but classified as different
            //
            // Tolerance trade-offs:
            // - smaller tolerance -> More false negatives
            // - larger tolerance -> more false positives
            //
            // the difference between theoretically equal values vary widely depending on the context. floats are saved with 1e-7 precision but errors can add up to rather large values large for e.g. physics and especially finance calculations. for ZHunter it should usually be very small though
            // so if not particularly worried about precision, since false negatives are hard to gauge, I would recommend to think about false positives: what values close to each other would I be comfortable with being the same (across different OoM). 
            // then chose tolerance values that are just as big as you are comfortable with. that way, we get an as good as it gets negative rejection  
            
            const float absoluteTolerance = 1e-5f; // for very small values
            const float relativeTolerance = 1e-6f; // for larger values
            
            return Math.Max(absoluteTolerance, Math.Max(Math.Abs(value1), Math.Abs(value2)) * relativeTolerance); // = max(absoluteTolerance, relativeTolerance * largerFloat)
        }
        public static bool IsSameDoubleValue(double value1, double value2) => Math.Abs(value1 - value2) <= GetDoubleTolerance(value1, value2);
        public static bool IsLesserEqualThanDouble(this double value1, double value2) => value1 < value2 + GetDoubleTolerance(value1, value2);
        public static bool IsGreaterEqualThanDouble(this double value1, double value2) => value1 > value2 - GetDoubleTolerance(value1, value2);

        private static double GetDoubleTolerance(double value1, double value2)
        {
            // doubles are saved with 1e-15 or 1e-16 precision. lets not completely over-engineer this, just use 1e3 smaller tolerances and be done with it 
            const double absoluteTolerance = 1e-8;
            const double relativeTolerance = 1e-9;

            return Math.Max(absoluteTolerance, Math.Max(Math.Abs(value1), Math.Abs(value2)) * relativeTolerance);
        }
        
        public static Dictionary<Enum, TKey> ConvertKeysStringToEnum<TKey>(this Dictionary<string, TKey> originalDict, params Type[] possibleEnumTypes)
        {
            Dictionary<Enum, TKey> convertedDict = new();
            
            foreach (string stringKey in originalDict.Keys)
            {
                Enum enumKey = stringKey.ConvertStringToEnum(possibleEnumTypes);
                if (enumKey == null) return null;
                
                convertedDict.Add(enumKey, originalDict[stringKey]);
            }
            
            return convertedDict;
        }
        
        public static Dictionary<TKey, Enum> ConvertValuesStringToEnum<TKey>(this Dictionary<TKey, string> originalDict, params Type[] possibleEnumTypes)
        {
            Dictionary<TKey, Enum> convertedDict = new();
            
            foreach (KeyValuePair<TKey, string> originalEntry in originalDict)
            {
                Enum parsedEnum = originalEntry.Value.ConvertStringToEnum(possibleEnumTypes);
                if (parsedEnum == null) return null;
                
                convertedDict.Add(originalEntry.Key, parsedEnum);
            }
            
            return convertedDict;
        }
        
        public static Enum ConvertStringToEnum(this string inputString, params Type[] possibleEnumTypes)
        {
            Enum enumKey = null;
            bool hasBeenParsedSuccessfully = false;
            foreach (Type type in possibleEnumTypes)
            {
                try
                {
                    enumKey = (Enum)Enum.Parse(type, inputString);
                    if (hasBeenParsedSuccessfully) 
                    {
                        $"Key {inputString} is ambiguous. Returning null.".Log(level: ZMethodsDebug.LogLevel.Warning);
                        return null;
                    } 
                    hasBeenParsedSuccessfully = true;
                }
                catch (ArgumentException) { }
            }

            if (!hasBeenParsedSuccessfully)
            {
                Debug.LogWarning($"ZMethods.{MethodBase.GetCurrentMethod()}: Could not convert key {inputString} to any of the possible enum types. returning null.");
                return null;
            }
            
            return enumKey;
        }
        
        public static List<Vector2Int> NonDefaultToIndexList<T>(this T[,] array)
        {
            List<Vector2Int> result = new();
        
            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    if (!IsDefaultValue(array[x, y])) result.Add(new Vector2Int(x, y));
                }
            }
        
            return result;
        }
        
        public static void AddOrIncrement<TKey>(this Dictionary<TKey, int> dictionary, TKey key, int value) where TKey : notnull
        {
            if (dictionary.TryGetValue(key, out int currentCount)) dictionary[key] = currentCount + value;
            else dictionary[key] = value;
        }
        
        // public static List<Vector2Int> NonDefaultToIndexList<T>(T[,] array) => FilterValueToIndexListCommon(array, default, filterOutValue: true); // TODO does this work?
        public static List<Vector2Int> FilterOutValueToIndexList<T>(this T[,] array, T filterValue) => FilterValueToIndexListCommon(array, filterValue, filterOutValue: true);
        public static List<Vector2Int> FilterByValueToIndexList<T>(this T[,] array, T filterValue) => FilterValueToIndexListCommon(array, filterValue, filterOutValue: false);
        private static List<Vector2Int> FilterValueToIndexListCommon<T>(T[,] array, T filterValue, bool filterOutValue)
        {
            List<Vector2Int> result = new();
        
            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    if (IsSameValue(array[x, y], filterValue) != filterOutValue) result.Add(new Vector2Int(x, y));
                }
            }
        
            return result;
        }
        
        public static void AddToByte(ref byte currentValue, int amount)
        {
            int result = currentValue + amount;

            if (result < byte.MinValue) currentValue = byte.MinValue;
            else if (result > byte.MaxValue) currentValue = byte.MaxValue; 
            else currentValue = (byte)result;
        }
        
        public static bool IndexIsInRange(this int index, int arraySize)
        {
            return (index >= 0 && index < arraySize);
        }

        public static List<Vector2Int> GetNeighborIndices(this Vector2Int vertex, int range = 1)
        {
            List<Vector2Int> neighboringVertices = new();

            for (int dy = -range; dy <= range; dy++)
            {
                for (int dx = -range; dx <= range; dx++)
                {
                    neighboringVertices.Add(vertex + new Vector2Int(dx, dy));
                }
            }

            return neighboringVertices;
        }
        
        public static bool TryCast<TTarget>(object input, out TTarget result, bool verbose = true)
        {
            if (input is TTarget cast)
            {
                result = cast;
                return true;
            }
            
            if (verbose)
            {
                MethodBase callingMethod = new System.Diagnostics.StackFrame(1, true).GetMethod();
                string classNameString = callingMethod?.ReflectedType?.Name;
                string methodNameString = callingMethod?.Name;
                string inputTypeString = (input == null) ? "null" : input.GetType().ToString();
                Debug.LogWarning($"{classNameString}.{methodNameString}: Unable to cast type {inputTypeString} to type {typeof(TTarget)}.");
            }
            
            result = default;
            return false;
        }
    }
}