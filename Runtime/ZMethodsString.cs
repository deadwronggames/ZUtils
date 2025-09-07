using System;
using System.Text;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsString
    {
        public static string GetPercentString(float value, bool doSign = false, bool isZeroNegative = false, int decimalPlaces = 0)
        {
            // determine if and what sign should be shown
            string signString = (doSign) ?
                ZMethods.IsSameFloatValue(value, 0f) ?
                    (isZeroNegative) ?
                        "-" :
                        "+" :
                    value.IsGreaterEqualThanFloat(0f) ?
                        "+" :
                        "-" :
                "";
                
            // round the value to the specified number of decimal places
            float roundedValue = (float)Math.Round(100 * Math.Abs(value), decimalPlaces);
            string formattedValue = roundedValue.ToString("F" + decimalPlaces);
            
            // final string
            return signString + formattedValue + "%";
        }

        public static string IntToRomanNumeralString(this int number)
        {
            if (number is < 1 or > 3999)
            {
                Debug.LogWarning($"{nameof(ZMethodsString)}.{nameof(IntToRomanNumeralString)}: Number {number} is outside of the standard roman numbers range [1, 3999]. Returning arabic number string.");
                return number.ToString();
            }

            (int value, string numeral)[] map = {
                (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
                (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
                (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
            };

            StringBuilder result = new();
            foreach ((int value, string numeral) in map)
            {
                while (number >= value)
                {
                    result.Append(numeral);
                    number -= value;
                }
            }

            return result.ToString();
        }
    }
}