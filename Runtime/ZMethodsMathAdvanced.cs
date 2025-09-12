using System;
using System.Linq;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsMathAdvanced
    {
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
                if (ZMethods.IsSameFloatValue(Math.Abs(coefficientsAndResult[pivotRow, i]), 0f)) // check for zero pivot
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
    }
}