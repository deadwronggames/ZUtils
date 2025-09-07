using UnityEngine;
using UnityEngine.UI;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsTween
    {
        // TODO use tween library
        
        // public static Tween ShrinkHeightRectTransform(RectTransform rectTransformToShrink, float duration, RectTransform rectTransformParentLayoutGroup = null, bool doDestroyAfterShrink = true, Ease ease = Ease.Linear)
        // {
        //     // shrink height to zero then destroy
        //     Action onCompleteAction = (doDestroyAfterShrink) ? () => UnityEngine.Object.Destroy(rectTransformToShrink.gameObject) : EmptyAction;
        //     Tween shrinkTween = rectTransformToShrink
        //         .DOSizeDelta(new Vector2(rectTransformToShrink.sizeDelta.x, 0), duration)
        //         .SetEase(ease)
        //         .OnComplete(onCompleteAction.Invoke);
        // 
        //     // force layout update during shrinking
        //     if (rectTransformParentLayoutGroup != null) shrinkTween.OnUpdate(() => { LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransformParentLayoutGroup); });
        // 
        //     return shrinkTween;
        // }
        // 
        // public static float RemainingTime(this Tween tween) => (tween == null) ? 0f : tween.Duration() * (1 - tween.ElapsedPercentage());
    }
}
