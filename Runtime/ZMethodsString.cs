using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        
        public static string ReplaceIntegersInString(this string input, int newNumber)
        {
            const string pattern = @"\d+";
            return Regex.Replace(input, pattern, replacement: newNumber.ToString());
        }
        
        public static string ReplaceLastIntegerInString(this string input, int newNumber)
        {
            const string pattern = @"\d+";
            MatchCollection matches = Regex.Matches(input, pattern);
            if (matches.Count == 0) return input;

            Match lastMatch = matches[^1];

            return Regex.Replace(input, pattern, match => (match.Index == lastMatch.Index) ? newNumber.ToString() : match.Value); // replace last match, leave others unchanged
        }
        
        public static string FormattedInspectorString(string stringToConvert)
        {
            stringToConvert = stringToConvert.Replace("\\n", "\n");
            stringToConvert = stringToConvert.Replace("\\t", "\t");
            stringToConvert = stringToConvert.Replace("\\v", "\v");
            stringToConvert = stringToConvert.Replace("\\u202F", "\u202F"); // allows no line break (basically an mbox around word before and after)
            stringToConvert = stringToConvert.Replace("\\u2009", "\u2009"); // allows line break
            
            return stringToConvert;
        }
        
        public static string ColorizeString(this string stringToColorize, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{stringToColorize}</color>";
        public static string ResizeString(this string stringToColorize, int fontSize) => $"<size={fontSize}>{stringToColorize}</size>";
        
        public static string DropFirstWord(this string input)
        {
            int indexSecondWord = 0;
            foreach (char letter in input[1..]) // drop the first letter
            {
                indexSecondWord++;
                if (char.IsUpper(letter)) break;
            }

            return input[indexSecondWord..];
        }
        
        public static int CountOccurenceOfSpecificCharacters(this string input, params char[] characters) => (string.IsNullOrEmpty(input)) ? 0 : input.Count(characters.Contains);
    }
}