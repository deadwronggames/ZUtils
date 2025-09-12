using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsMath
    {
        public static float EvaluateExponential(float x, float yOffset = 0f, float xOffset = 0f, float amplitude = 1f, float growthFactor = 1f)
        {
            return yOffset + amplitude * (float)Math.Exp(growthFactor * x + xOffset);
        }
        
        public static float EvaluateSigmoid(float x, float yOffset = 0, float xOffset = 0f, float amplitude = 1f, float steepness = 1f)
        {
            return yOffset + amplitude / (1 + (float)Math.Exp(-steepness * (x - xOffset)));
            
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
    }
}