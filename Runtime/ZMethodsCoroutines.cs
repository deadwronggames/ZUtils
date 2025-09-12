using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsCoroutines
    {
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
    }
}