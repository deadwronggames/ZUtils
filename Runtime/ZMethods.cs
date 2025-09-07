using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethods
    {
        private const bool USE_CONST_SEED = false; // for debugging
        private static readonly System.Random s_seedGenerator = new();
        private static readonly System.Random s_random = GetSystemRandom();
        
        public static void DestroyAllChildren(this Transform transform)
        {
            foreach (Transform child in transform) UnityEngine.Object.Destroy(child.gameObject);
        }

        public static Transform[] GetAllChildrenTransforms(Transform transform)
        {
            return transform.Cast<Transform>().ToArray();
        }
        
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
        
        public static Action EmptyAction { get; } = () => { };
        
        public static Vector2 ScreenToWorld(Vector2 screenPosition, Camera camera) => camera.ScreenToWorldPoint(screenPosition);
      
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

        
        
        public static float NormalizeAngleTo2Pi(this float angle)
        {
            const float twoPi = 2 * Mathf.PI;
            return (angle % twoPi + twoPi) % twoPi;
        }
        
        public static float GetRadiansAngleDifference(float angle1, float angle2)
        {
            angle1 = NormalizeAngleTo2Pi(angle1);
            angle2 = NormalizeAngleTo2Pi(angle2);

            float diff = Mathf.Abs(angle1 - angle2);
            diff = Mathf.Min(diff, 2 * Mathf.PI - diff);

            return diff;
        }
        
        // Calculate the cross product of vectors (p1-p2) and (p3-p2). returns 0 if in onw line, negative if CW and positive if CCW (or other way around)
        public static float CrossProduct(Vector2 p1, Vector2 p2, Vector2 p3) { return (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x); }
        
        public static float CalculateAngle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Vector2 vectorAB = p1 - p2;
            Vector2 vectorBC = p3 - p2;

            float dotProduct = Vector2.Dot(vectorAB.normalized, vectorBC.normalized);
            float angle = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f)) * Mathf.Rad2Deg;

            // Check the sign of the cross product to determine if the angle is > 180 degrees
            float crossProduct = vectorAB.x * vectorBC.y - vectorAB.y * vectorBC.x;
            if (crossProduct < 0f) angle = 360f - angle;

            return angle;
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
                        Debug.LogWarning($"ZMethods.{MethodBase.GetCurrentMethod()}: Key {inputString} is ambiguous. returning null.");
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
        
        public static string ReplaceIntegersInString(this string input, int newNumber)
        {
            const string pattern = @"\d+";
            return Regex.Replace(input, pattern, newNumber.ToString());
        }
        
        public static string ReplaceLastIntegerInString(this string input, int newNumber)
        {
            const string pattern = @"\d+";
            MatchCollection matches = Regex.Matches(input, pattern);
            if (matches.Count == 0) return input;

            Match lastMatch = matches[^1];

            return Regex.Replace(input, pattern, match => (match.Index == lastMatch.Index) ? newNumber.ToString() : match.Value); // replace last match, leave others unchanged
        }
        
        // simple wrapper just because I prefer this style and with some safety checks
        public static string ZFormat(this string template, params object[] args)
        {
            int numberPlaceholders = template.CountPlaceholders();
            if (numberPlaceholders > args.Length)
            {
                if (Application.isPlaying) // otherwise it might just be wonky spam
                    Debug.LogWarning($"{nameof(ZFormat)}: String \"{template}\" has {numberPlaceholders} place holders but {args.Length} format arguments where passed. Returning unformatted string.");
                return template;
            }
            
            return string.Format(template, args);
        }
        
        // match regular expression for placeholders like {0}, {1}, etc.
        public static int CountPlaceholders(this string input) => (!string.IsNullOrEmpty(input)) ? Regex.Matches(input, @"\{\d+\}").Count : 0;
        
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

        public static string ToPercentString(this float value) => $"{100 * value}%";
        
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
        
        
        public static Color ModifyAlpha(Color color, float alphaValue)
        {
            color.a = alphaValue;
            return color;
        }
        
        public static float EvaluateExponential(float x, float yOffset = 0f, float xOffset = 0f, float amplitude = 1f, float growthFactor = 1f)
        {
            return yOffset + amplitude * (float)Math.Exp(growthFactor * x + xOffset);
        }
        
        public static float EvaluateSigmoid(float x, float yOffset = 0, float xOffset = 0f, float amplitude = 1f, float steepness = 1f)
        {
            return yOffset + amplitude / (1 + (float)Math.Exp(-steepness * (x - xOffset)));
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
        
        public static void AddToByte(ref byte currentValue, int amount)
        {
            int result = currentValue + amount;

            if (result < byte.MinValue) currentValue = byte.MinValue;
            else if (result > byte.MaxValue) currentValue = byte.MaxValue; 
            else currentValue = (byte)result;
        }

        public static void ChangeLayer(GameObject gameObject, string layerName)
        {
            int newLayerIndex = LayerMask.NameToLayer(layerName);
            if (newLayerIndex != -1) gameObject.layer = newLayerIndex;
            else Debug.LogError("Layer not found: " + layerName);
        }
        
        public static int GetLayer(string layerName)
        {
            int newLayerIndex = LayerMask.NameToLayer(layerName);
            if (newLayerIndex != -1) return newLayerIndex;
            Debug.LogError("Layer not found: " + layerName);
            return -1;
        }

        public static int GetLayerMask(params string[] layerNames)
        {
            int layerMask = 0;

            foreach (string layerName in layerNames)
            {
                int layer = LayerMask.NameToLayer(layerName);
                if (layer != -1) layerMask |= 1 << layer; 
                else  Debug.LogError($"Layer name '{layerName}' is not valid.");
            }

            return layerMask;
        }

        public static void DelayedByOneFrameAction(MonoBehaviour owner, Action action, bool isOnlyCountingWhenTimeScaleNotZero = false) => DelayedByFramesAction(owner, 1, action, isOnlyCountingWhenTimeScaleNotZero);
        public static void DelayedByFramesAction(MonoBehaviour owner, int delayFrames, Action action, bool isOnlyCountingWhenTimeScaleNotZero = false)
        {
            owner.StartCoroutine(DelayedActionCR());
            return;

            IEnumerator DelayedActionCR()
            {
                do {
                    yield return null;
                    if (!isOnlyCountingWhenTimeScaleNotZero || Time.timeScale > 0f) delayFrames--;
                } while (delayFrames > 0);
                action.Invoke();
            }
        }
        public static Coroutine DelayedAction(MonoBehaviour owner, float delay, Action action, bool realtime = false)
        {
            return owner.StartCoroutine(DelayedActionCR());
            
            IEnumerator DelayedActionCR()
            {
                if (delay.IsLesserEqualThanFloat(0f)) yield return null;
                else if (realtime) yield return GetWaitForSecondsRealtime(delay);
                else yield return GetWaitForSeconds(delay);
                action.Invoke();
            }
        }
        
        public static Coroutine RepeatedAction(MonoBehaviour owner, float interval, Action action, bool realtime = false)
        {
            return owner.StartCoroutine(RepeatedActionCR());
            
            IEnumerator RepeatedActionCR()
            {
                while(Application.isPlaying)
                {
                    action.Invoke();
                    if (interval < float.Epsilon) yield return null;
                    else if (realtime) yield return GetWaitForSecondsRealtime(interval);
                    else yield return GetWaitForSeconds(interval);
                }
            }
        }
        
        public static void StopCoroutine(MonoBehaviour owner, ref Coroutine coroutineToStop)
        {
            if (coroutineToStop == null) return;
            
            owner.StopCoroutine(coroutineToStop);
            coroutineToStop = null;
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

        public static List<Vector2Int> DropVectorsInsideRadius(this IEnumerable<Vector2Int> vectors, Vector2Int centerPoint, float radius = 1f)
        {
            float radiusSquared = radius * radius;
            return vectors.Where(v => (v - centerPoint).sqrMagnitude >= radiusSquared).ToList();
        }
        public static List<Vector2> DropVectorsInsideRadius(this IEnumerable<Vector2> vectors, Vector2 centerPoint, float radius = 1f)
        {
            float radiusSquared = radius * radius;
            return vectors.Where(v => (v - centerPoint).sqrMagnitude >= radiusSquared).ToList();
        }
        
        public static List<Vector2Int> DropVectorsOutsideRadius(this IEnumerable<Vector2Int> vectors, Vector2Int centerPoint, float radius = 1f)
        {
            float radiusSquared = radius * radius;
            return vectors.Where(v => (v - centerPoint).sqrMagnitude < radiusSquared).ToList();
        }
        public static List<Vector2> DropVectorsOutsideRadius(this IEnumerable<Vector2> vectors, Vector2 centerPoint, float radius = 1f)
        {
            float radiusSquared = radius * radius;
            return vectors.Where(v => (v - centerPoint).sqrMagnitude < radiusSquared).ToList();
        }

        private static readonly Dictionary<float, WaitForSeconds> s_waitDict = new();
        public static WaitForSeconds GetWaitForSeconds(float time)
        {
            if (s_waitDict.TryGetValue(time, out WaitForSeconds wait)) return wait;

            s_waitDict[time] = new WaitForSeconds(time);
            return s_waitDict[time];
        }
        
        private static readonly Dictionary<float, WaitForSecondsRealtime> s_waitRealtimeDict = new();
        public static WaitForSecondsRealtime GetWaitForSecondsRealtime(float time)
        {
            if (s_waitRealtimeDict.TryGetValue(time, out WaitForSecondsRealtime wait)) return wait;

            s_waitRealtimeDict[time] = new WaitForSecondsRealtime(time);
            return s_waitRealtimeDict[time];
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
        
        public static string FormattedInspectorString(string stringToConvert)
        {
            stringToConvert = stringToConvert.Replace("\\n", "\n");
            stringToConvert = stringToConvert.Replace("\\t", "\t");
            stringToConvert = stringToConvert.Replace("\\v", "\v");
            stringToConvert = stringToConvert.Replace("\\u202F", "\u202F"); // allows no line break (basically an mbox around word before and after)
            stringToConvert = stringToConvert.Replace("\\u2009", "\u2009"); // allows line break
            
            return stringToConvert;
        }
        
        public static string ColorizeString(this string stringToColorize, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{stringToColorize}</color>";
        public static string ResizeString(this string stringToColorize, int fontSize) => $"<size={fontSize}>{stringToColorize}</size>";
        
        public static string DropFirstWord(this string input)
        {
            int indexSecondWord = 0;
            foreach (char letter in input[1..]) // drop the first letter
            {
                indexSecondWord++;
                if (char.IsUpper(letter)) break;
            }

            return input[indexSecondWord..];
        }
        
        public static int CountOccurenceOfSpecificCharacters(this string input, params char[] characters) => (string.IsNullOrEmpty(input)) ? 0 : input.Count(characters.Contains);
        
        public static bool TrySolveLinearEquation(float[,] coefficients, float[] result, out float[] solution)
        {
            int rows = coefficients.GetLength(0);
            int cols = coefficients.GetLength(1);
            
            solution = null;
            if (rows != result.Length || cols > rows) return false;
            
            // augment the coefficient matrix with the result vector
            float[,] augmentedMatrix = new float[rows, cols + 1];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++) 
                    augmentedMatrix[i, j] = coefficients[i, j];
                
                augmentedMatrix[i, cols] = result[i];
            }

            return TrySolveLinearEquation(augmentedMatrix, out solution);
        }

        /// <summary>
        /// see "(continuous time) markov chains", "steady state indexWeights" and this: https://www.probabilitycourse.com/chapter11/11_3_3_the_generator_matrix.php  
        /// </summary>
        /// <param name="holdingTimes">Average time spent in a state</param>
        /// <param name="transitionProbabilities">Transitions FROM the first state in the first column, TO the first state in the first row</param>
        public static float[] CalculateTimeMarkovSteadyStateProbabilities(float[] holdingTimes, float[,] transitionProbabilities)
        {
            int numberStates = holdingTimes.Length;
            
            // validate
            if (numberStates != transitionProbabilities.GetLength(0) || numberStates != transitionProbabilities.GetLength(1))
            {
                Debug.LogWarning($"{nameof(ZMethods)}.{nameof(CalculateTimeMarkovSteadyStateProbabilities)}: Dimensions do not match. Returning null");
                return null;
            }
            if (holdingTimes.Any(time => time == 0f))
            {
                Debug.LogWarning($"{nameof(ZMethods)}.{nameof(CalculateTimeMarkovSteadyStateProbabilities)}: Holding time of zero does not make sense. Returning null");
                return null;
            }
            
            // calculate the generator matrix:
            // - divide each entry by holding time
            // - diagonal is the flow out of the state. therefore, minus sign and (1 - p(self transition))
            float[,] generatorMatrix = new float[numberStates, numberStates];
            for (int i = 0; i < numberStates; i++)
                for (int j = 0; j < numberStates; j++)
                {
                    if (i == j) generatorMatrix[i, i] = -1 * (1f - transitionProbabilities[i, i]) / holdingTimes[j];
                    else generatorMatrix[i, j] = transitionProbabilities[i, j] / holdingTimes[j];
                }
            
            return CalculateMarkovSteadyStateProbabilitiesCommon(generatorMatrix);
        }
        
        /// <param name="transitionProbabilities">Transitions FROM the first state in the first column, TO the first state in the first row</param>
        public static float[] CalculateMarkovSteadyStateProbabilities(float[,] transitionProbabilities)
        {
            int numberStates = transitionProbabilities.GetLength(0);
            
            if (numberStates != transitionProbabilities.GetLength(1))
            {
                Debug.LogWarning($"{nameof(ZMethods)}.{nameof(CalculateMarkovSteadyStateProbabilities)}: Dimensions do not match. Returning null.");
                return null;
            }
            
            // create transition matrix: diagonal represents flow out of the state
            float[,] transitionMatrix = new float[numberStates, numberStates];
            for (int i = 0; i < numberStates; i++)
                transitionMatrix[i, i] = -1 * (1f - transitionProbabilities[i, i]);

            return CalculateMarkovSteadyStateProbabilitiesCommon(transitionMatrix);
        }
        private static float[] CalculateMarkovSteadyStateProbabilitiesCommon(float[,] transitionMatrix)
        {
            int numberStates = transitionMatrix.GetLength(0);
            float[,] coefficientsAndResultMatrix = new float[numberStates + 1, numberStates + 1];
            
            // copy values from transitionMatrix to coefficientsAndResultMatrix
            for (int i = 0; i < numberStates; i++)
                for (int j = 0; j < numberStates; j++)
                    coefficientsAndResultMatrix[i, j] = transitionMatrix[i, j];

            // fill the last column with zeros for result because flow in needs to equal flow out of state
            for (int i = 0; i < numberStates + 1; i++)
                coefficientsAndResultMatrix[i, numberStates] = 0f;

            // fill the last row with ones for normalization condition
            for (int j = 0; j < numberStates + 1; j++)
                coefficientsAndResultMatrix[numberStates, j] = 1f;
            
            bool isSuccess = TrySolveLinearEquation(coefficientsAndResultMatrix, out float[] solution);
            if (!isSuccess)
            {
                Debug.LogWarning($"{nameof(ZMethods)}.{nameof(CalculateMarkovSteadyStateProbabilities)}: Steady state equations where no solvable. Returning null.");
                return null;
            }
            
            return solution;
        }
        
        public static bool TrySolveLinearEquation(float[,] coefficientsAndResult, out float[] solution)
        {
            int rows = coefficientsAndResult.GetLength(0);
            int cols = coefficientsAndResult.GetLength(1) - 1; // -1 for result
            
            solution = null;
            if (cols > rows) return false;
    
            // perform gauss elimination - careful, does not scale great with high dimensions
            for (int i = 0; i < Math.Min(rows, cols); i++)
            {
                // find the pivot element
                int pivotRow = i;
                for (int j = i + 1; j < rows; j++) 
                    if (Math.Abs(coefficientsAndResult[j, i]) > Math.Abs(coefficientsAndResult[pivotRow, i])) 
                        pivotRow = j;
    
                // swap pivot row with current row
                if (IsSameFloatValue(Math.Abs(coefficientsAndResult[pivotRow, i]), 0f)) // check for zero pivot
                    return false;
                
                if (pivotRow != i)
                    for (int j = 0; j < cols + 1; j++) 
                        (coefficientsAndResult[i, j], coefficientsAndResult[pivotRow, j]) = (coefficientsAndResult[pivotRow, j], coefficientsAndResult[i, j]);
    
                // eliminate entries below the pivot
                for (int j = i + 1; j < rows; j++)
                {
                    float factor = coefficientsAndResult[j, i] / coefficientsAndResult[i, i];
                    for (int k = i; k < cols + 1; k++)
                        coefficientsAndResult[j, k] -= factor * coefficientsAndResult[i, k];
                }
            }
    
            // back substitution
            solution = new float[cols];
            for (int i = Math.Min(rows, cols) - 1; i >= 0; i--)
            {
                float sum = coefficientsAndResult[i, cols];
                for (int j = i + 1; j < cols; j++)
                    sum -= coefficientsAndResult[i, j] * solution[j];
                
                solution[i] = sum / coefficientsAndResult[i, i];
            }
    
            return true; 
        }
        
        public static float[] CalculateSteadyStateProbabilities(float[] durations, float[,] transitionMatrix)
        {
            int numberOfStates = durations.GetLength(0);
            float[] steadyState = new float[numberOfStates];
            float[,] matrix = new float[numberOfStates, numberOfStates + 1];
        
            // Create augmented matrix for solving the linear equations
            for (int i = 0; i < numberOfStates; i++)
            {
                for (int j = 0; j < numberOfStates; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = 1 - transitionMatrix[i, j]; // 1 - p_ii
                    }
                    else
                    {
                        matrix[i, j] = -transitionMatrix[i, j]; // -p_ij
                    }
                }
                matrix[i, numberOfStates] = 0; // Right-hand side
            }
        
            // Add normalization condition
            for (int i = 0; i < numberOfStates; i++)
            {
                matrix[numberOfStates - 1, i] = 1; // π_1 + π_2 + ... + π_n = 1
            }
            matrix[numberOfStates - 1, numberOfStates] = 1;
        
            // Solve the linear system using Gaussian elimination
            for (int i = 0; i < numberOfStates; i++)
            {
                // Make the diagonal contain all 1's
                float diagElement = matrix[i, i];
                for (int j = 0; j < numberOfStates + 1; j++)
                {
                    matrix[i, j] /= diagElement;
                }
        
                // Eliminate other rows
                for (int k = 0; k < numberOfStates; k++)
                {
                    if (k != i)
                    {
                        float factor = matrix[k, i];
                        for (int j = 0; j < numberOfStates + 1; j++)
                        {
                            matrix[k, j] -= factor * matrix[i, j];
                        }
                    }
                }
            }
        
            // Retrieve the steady-state indexWeights
            for (int i = 0; i < numberOfStates; i++)
            {
                steadyState[i] = matrix[i, numberOfStates];
            }
        
            return steadyState;
        }
        
        public static void Print(this object obj, string prefix = null)
        {
            string prefixString = (!string.IsNullOrEmpty(prefix)) ? prefix + ": " : "";
            Debug.Log(prefixString + obj);
        }
        public static void PrintIEnumerable<TEntry>(this IEnumerable<TEntry> entries, string name = null)
        {
            List<TEntry> entryList = entries.ToList();
            int numberOfEntries = entryList.Count;
            
            string entryPluralizedString = (numberOfEntries == 1) ? "entry" : "entries";
            Debug.Log($"{(!string.IsNullOrEmpty(name) ? name : "Enumerable")} has {numberOfEntries} {entryPluralizedString}");
            for (int i = 0; i < numberOfEntries; i++) Debug.Log($"'--- Index {i}: {entryList[i]}");
        }
        public static void PrintDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, string name = null)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                Debug.Log($"{(!string.IsNullOrEmpty(name) ? name : "Dictionary")} is empty.");
                return;
            }

            Debug.Log($"{(!string.IsNullOrEmpty(name) ? name : "Dictionary")} contains {dictionary.Count} entries:");
            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
            {
                if (kvp.Value is IEnumerable valueEnumerable && kvp.Value is not string)
                {
                    // convert to a list to check emptiness
                    List<object> valueList = valueEnumerable.Cast<object>().ToList();
                    if (valueList.Count == 0) Debug.Log($"--- {kvp.Key}: Empty {kvp.Value.GetType().Name}");
                    else {
                        Debug.Log($"--- {kvp.Key}:");
                        foreach (object subValue in valueList)
                            Debug.Log($"------ {subValue}");
                    }
                }
                else Debug.Log($"--- {kvp.Key}: {kvp.Value}");
            }
        }


        public static int DebugShowAnimationAtFrame(Animator animator, string animationName, int frame)
        {
            AnimationClip animationClip = animator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == animationName);
            if (animationClip == null)
            {
                Debug.LogWarning($"{animationName} not found.");
                return -1;
            }
            float totalFrames = 60 * animationClip.length;
            
            frame = (int)(frame % (totalFrames + 1));
            Debug.Log($"Frame {frame} of {totalFrames}.");
            
            animator.speed = 0;
            animator.Play(animationName, -1, frame / totalFrames);
            return frame + 1;
        }
    }
}