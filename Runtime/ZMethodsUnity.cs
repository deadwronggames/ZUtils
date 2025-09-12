using System.Linq;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsUnity
    {
        public static void DestroyAllChildren(this Transform transform)
        {
            foreach (Transform child in transform) Object.Destroy(child.gameObject);
        }

        public static Transform[] GetAllChildrenTransforms(this Transform transform)
        {
            return transform.Cast<Transform>().ToArray();
        }
        
        public static Vector2 ScreenToWorld(this Vector2 screenPosition, Camera camera) => camera.ScreenToWorldPoint(screenPosition);
        
        public static void ChangeLayer(this GameObject gameObject, string layerName)
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
    }
}