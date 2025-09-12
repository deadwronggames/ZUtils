using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsDebug
    {
        public enum LogLevel
        {
            Critical,
            Error,
            Warning,
            Info,
            Verbose
        }

        public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Info;

        // Add more as needed. CAREFUL! also add to dictionary below
        public enum LogCategory
        {
            General,
        }

        // Runtime toggle for categories
        // TODO could make dictionary with default true in case one forgets to add new category
        private static readonly Dictionary<LogCategory, bool> s_categoryEnabled = new()
        {
            { LogCategory.General, true },
        };

        // Keep track of logs for LogOnce
        private static readonly HashSet<int> s_loggedOnce = new();

        // file logging
        private static readonly string s_logFilePath = Path.Combine(Application.persistentDataPath, "ZDebug.log");
    
        /// <summary>Enable or disable logging for a category</summary>
        public static void CategorySetEnabled(LogCategory category, bool isEnabled) => s_categoryEnabled[category] = isEnabled;
    
        /// <summary>Main log function</summary>
        public static void Log(this object obj, LogCategory category = LogCategory.General, LogLevel level = LogLevel.Info, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
            => LogInternal(obj, category, level, file, member, line);
    
        private static void LogInternal(this object obj, LogCategory category = LogCategory.General, LogLevel level = LogLevel.Info, string file = "", string member = "", int line = 0)
        {
            if (level > CurrentLogLevel) return;
            if (s_categoryEnabled.TryGetValue(category, out bool enabled) && !enabled) return;

            // Build prefix string    
            string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string className = Path.GetFileNameWithoutExtension(file);
            string logInfo = $"[{timeStamp}] [{category}] [{level}]";
            string sourceInfo = $"In {className}.{member} (line {line}):";

            // Choose console color (only for Unity console, not file)
            string coloredLogInfo = level switch
            {
                LogLevel.Critical => $"<color=magenta>{logInfo}</color>",
                LogLevel.Error    => $"<color=red>{logInfo}</color>",
                LogLevel.Warning  => $"<color=yellow>{logInfo}</color>",
                LogLevel.Verbose  => $"<color=gray>{logInfo}</color>",
                _                 => logInfo
            };

            // Unity console message
            string unityMessage = $"{coloredLogInfo} {sourceInfo}\n{obj}\n";
            switch (level)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    Debug.LogError(unityMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(unityMessage);
                    break;
                case LogLevel.Info:
                case LogLevel.Verbose:
                    Debug.Log(unityMessage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

#if !UNITY_EDITOR
        // Emit to file (without color tags)
        throw new NotImplementedException();
            
        // TODO implement but make sure file does not get too big (see TryWriteFile in ZMethodsFileIO)
        // string logFileMessage = $"{logInfo} {sourceInfo}{Environment.NewLine}{message}{Environment.NewLine}";
        // try { File.AppendAllText(s_logFilePath, logFileMessage); }
        // catch { /* ignore file errors */ }
#endif
        }
        
        /// <summary>Log a message only once per session</summary>
        public static void LogOnce(this object obj, LogCategory category = LogCategory.General, LogLevel level = LogLevel.Info, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            int key = HashCode.Combine(file, member, line, obj);
            if (s_loggedOnce.Contains(key)) return;
            s_loggedOnce.Add(key);
            LogInternal(obj, category, level, file, member, line);
        }

        /// <summary>Reset LogOnce tracker</summary>
        public static void ResetLogOnce() => s_loggedOnce.Clear();

        /// <summary>Draw a debug line in the world with optional color and duration</summary>
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f)
        {
            Debug.DrawLine(start, end, color, duration);
        }
    
        // TODO could maybe also allow to spawn a gizmo at a position

        /// <summary>Draw a debug ray in the world with optional color and duration</summary>
        public static void DrawRay(Vector3 origin, Vector3 direction, Color color, float duration = 0f)
        {
            Debug.DrawRay(origin, direction, color, duration);
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
                    // Convert to a list to check emptiness
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