using DG.Tweening;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsUnity
    {
        /// <summary>
        /// Returns the object itself if it exists, null otherwise.
        /// </summary>
        /// <remarks>
        /// This method helps differentiate between a null reference and a destroyed Unity object. Unity's "== null" check
        /// can incorrectly return true for destroyed objects, leading to misleading behaviour. The OrNull method use
        /// Unity's "null check", and if the object has been marked for destruction, it ensures an actual null reference is returned,
        /// aiding in correctly chaining operations and preventing NullReferenceExceptions.
        /// </remarks>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object being checked.</param>
        /// <returns>The object itself if it exists and not destroyed, null otherwise.</returns>
        public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;
        public static bool IsNull<T>(this T obj) where T : Object => obj; // TODO test
        
        public static void ForEachChild(this Transform parent, System.Action<Transform> action) 
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                action(parent.GetChild(i)); 
        }
        
        public static void DestroyChildren(this Transform parent) 
        {
            parent.ForEachChild(child => Object.Destroy(child.gameObject));
        }
        
        public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
            => new(x ?? vector.x, y ?? vector.y);
        
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
            => new(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        
        public static Vector2 ScreenToWorld(this Vector2 screenPosition, Camera camera) => camera.ScreenToWorldPoint(screenPosition);
        
        
        // RectTransforms

        public static void SetSize(this RectTransform rt, Vector2 newSize)
        {
            (bool isStretchingHorizontal, bool isStretchingVertical) = rt.IsStretching();
            
            if (isStretchingHorizontal && isStretchingVertical)
            {
                // Zero all offsets, let stretching do its thing
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
            }
            else if (isStretchingHorizontal)
            {
                // Only set height
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0f);
                rt.sizeDelta = new Vector2(0f, newSize.y);
            }
            else if (isStretchingVertical)
            {
                // Only set width
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
                rt.sizeDelta = new Vector2(newSize.x, 0f);
            }
            else
            {
                // Set height and width
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = newSize;
            }
        }
        
        /// <summary>
        /// Returns the actual width and height of a RectTransform, regardless of anchor/stretch.
        /// </summary>
        public static Vector2 GetActualSize(this RectTransform rt)
        {
            if (rt == null) return Vector2.zero;

            // If stretched, sizeDelta is offset from parent size
            Rect parentRect = rt.parent.GetComponent<RectTransform>().rect;
            float width = (rt.anchorMax.x - rt.anchorMin.x) * parentRect.width + rt.sizeDelta.x;
            float height = (rt.anchorMax.y - rt.anchorMin.y) * parentRect.height + rt.sizeDelta.y;

            return new Vector2(width, height);
        }

        /// <summary>
        /// Checks if the RectTransform is stretched horizontally and/or vertically.
        /// </summary>
        public static (bool horizontal, bool vertical) IsStretching(this RectTransform rt)
        {
            if (rt == null) return (false, false);
            bool stretchH = !ZMethods.IsSameFloatValue(rt.anchorMin.x, rt.anchorMax.x);
            bool stretchV = !ZMethods.IsSameFloatValue(rt.anchorMin.y, rt.anchorMax.y);
            return (stretchH, stretchV);
        }
        
        // Layers
        public static void SetLayer(this GameObject gameObject, string layerName)
        {
            int newLayerIndex = LayerMask.NameToLayer(layerName);
            if (newLayerIndex != -1) gameObject.layer = newLayerIndex;
            else $"Layer {layerName} not found. Doing nothing.".Log(level: ZMethodsDebug.LogLevel.Warning);
        }

        public static void SetLayersRecursively(this GameObject gameObject, string layerName)
            => SetLayersRecursively(gameObject, GetLayer(layerName));
        
        public static void SetLayersRecursively(this GameObject gameObject, int layer) 
        {
            gameObject.layer = layer;
            gameObject.transform.ForEachChild(child => child.gameObject.SetLayersRecursively(layer));
        }
        
        public static int GetLayer(string layerName)
        {
            int newLayerIndex = LayerMask.NameToLayer(layerName);
            if (newLayerIndex != -1) return newLayerIndex;
            $"Layer {layerName} not found. Returning -1. ".Log(level: ZMethodsDebug.LogLevel.Warning);
            return -1;
        }

        public static int GetLayerMask(params string[] layerNames)
        {
            int layerMask = 0;

            foreach (string layerName in layerNames)
            {
                int layer = LayerMask.NameToLayer(layerName);
                if (layer != -1) layerMask |= 1 << layer; 
                else  $"Layer {layerName} not found. Continuing.".Log(level: ZMethodsDebug.LogLevel.Warning);
            }

            return layerMask;
        }
       
        
        // Colors
        public static Color SetAlpha(this Color color, float alpha) => new(color.r, color.g, color.b, alpha);
    }
}