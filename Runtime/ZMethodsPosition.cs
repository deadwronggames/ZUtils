using System;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsPosition
    {
        public static Vector2[] GetRandomPositionsSpiral(int count, float distanceParameter, Vector2 centerPosition = default)
        {
            System.Random random = ZMethods.GetSystemRandom();
            Vector2[] positions = new Vector2[count];

            // generate the positions
            for (int i = 0; i < count; i++)
            {
                // calculate a random angle
                double angle = (random.NextDouble() * 2 * Mathf.PI); // in radians
            
                // dynamically increase the distance as the count increases to avoid crowding
                double distance = Math.Sqrt(i) * distanceParameter;
            
                // calculate the position based on polar coordinates
                Vector2 randomPosition = new(
                    (float)(Math.Cos(angle) * distance),
                    (float)(Math.Sin(angle) * distance)
                );

                positions[i] = centerPosition + randomPosition;
            }

            return positions;
        }
        
        public static Vector2[] GetRandomPositionsInSquare(int count, float xMin, float xMax, float yMin, float yMax)
        {
            Vector2[] positions = new Vector2[count];
            System.Random rand = ZMethods.GetSystemRandom();

            for (int i = 0; i < count; i++)
            {
                float x = (float)(rand.NextDouble() * (xMax - xMin) + xMin);
                float y = (float)(rand.NextDouble() * (yMax - yMin) + yMin);
                positions[i] = new Vector2(x, y);
            }

            return positions;
        }
    }
}