using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsTween
    {
        public static float RemainingTime(this Tween tween)
        {
            return (tween == null) ? 
                0f : 
                (1 - tween.ElapsedPercentage()) * tween.Duration();
        }
        
        public static void KillTweensRecursively(this Transform transform)  // TODO not sure if it maybe is already recursive
        {
            DOTween.Kill(transform.gameObject);
            transform.ForEachChild(KillTweensRecursively);
        }
        
        public static Tween DOShrinkHeightRectTransform(this RectTransform rt, float duration, RectTransform rectTransformParentLayoutGroup = null, bool doDestroyAfterShrink = true, Ease ease = Ease.Linear)
        {
            // shrink height to zero then destroy
            Action onCompleteAction = (doDestroyAfterShrink) ? () => UnityEngine.Object.Destroy(rt.gameObject) : ZMethods.EmptyAction;
            Tween shrinkTween = rt
                .DOSizeDelta(new Vector2(rt.sizeDelta.x, 0), duration)
                .SetEase(ease)
                .OnComplete(onCompleteAction.Invoke);
        
            // force layout update during shrinking
            if (rectTransformParentLayoutGroup != null) shrinkTween.OnUpdate(() => { LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransformParentLayoutGroup); });
        
            return shrinkTween;
        }
        
        // Wrappers for DOTween pro (Yes, I AM cheap!)
        public static Tween DOColor(this Graphic graphic, Color endValue, float duration) => DOTween.To(getter: () => graphic.color, setter: c => graphic.color = c, endValue, duration);
        public static Tween DOFillAmount(this Image image, float endValue, float duration) => DOTween.To(getter: () => image.fillAmount, setter: x => image.fillAmount = x, endValue, duration);
    }
}
