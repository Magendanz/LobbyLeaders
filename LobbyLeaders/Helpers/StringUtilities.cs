using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyList.Helpers
{
    /// <summary>
    /// String utility library
    /// </summary>
    public static class StringUtilities
    {
        public static double FuzzyMatch(string strA, string strB)
        {
            if (String.IsNullOrWhiteSpace(strA) || String.IsNullOrWhiteSpace(strB))
                return Double.NaN;

            // Split strings into trigrams
            var aGrams = SplitTrigrams(strA);
            var bGrams = SplitTrigrams(strB);

            // Count number of matches
            var result = 0d;
            foreach (var item in bGrams)
                if (aGrams.Contains(item))
                    ++result;

            // Note: This is different that standard normalized form, as it ignores extra trigrams
            return result / Math.Min(aGrams.Count, bGrams.Count);
        }

        public static List<string> SplitTrigrams(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            var result = new List<string>();
            var str = " " + value.ToUpperInvariant().Trim() + " ";

            for (var i = 0; i < str.Length - 2; ++i)
                result.Add(str.Substring(i, 3));

            return result;
        }

        #region Extension Methods
        public static bool Contains(this string str, string value, StringComparison comparisonType)
        {
            return str.IndexOf(value, comparisonType) >= 0;
        }

        public static string ToTitleCase(this string str)
        {
            var cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }
        #endregion Extension Methods
    }
}
