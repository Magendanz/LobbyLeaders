using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace LobbyLeaders.Helpers
{
    /// <summary>
    /// String utility library
    /// </summary>
    public static class StringExtensions
    {
        public static double FuzzyMatch(this string strA, string strB)
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

        public static List<string> SplitTrigrams(this string str)
        {
            if (String.IsNullOrWhiteSpace(str))
                return null;

            var result = new List<string>();
            str = " " + str.ToUpperInvariant().Trim() + " ";

            for (var i = 0; i < str.Length - 2; ++i)
                result.Add(str.Substring(i, 3));

            return result;
        }

        public static List<string> Variations(this string str, IEnumerable<IEnumerable<string>> abbreviations)
        {
            var words = str.ToAlphaNumeric().Trim().Split();
            var table = new List<List<string>>();

            foreach (var word in words)
            {
                var matches = new List<string>();
                matches.Add(word);

                foreach (var list in abbreviations)
                    foreach (var item in list)
                        if (String.Equals(item, word, StringComparison.OrdinalIgnoreCase))
                            matches.AddRange(list);

                table.Add(matches.Distinct().ToList());      // Eliminate duplicates
            }

            return Permutations(table);
        }

        private static List<string> Permutations(List<List<string>> table)
        {
            var list = table[0];
            table.RemoveAt(0);

            if (table.Count > 0)
            {
                var remnent = Permutations(table);
                var results = new List<string>();

                foreach (var word in list)
                    foreach (var item in remnent)
                        results.Add($"{word} {item}");

                return results;
            }
            else
                return list;
        }

        public static bool Contains(this string str, string value, StringComparison comparisonType)
            => str.IndexOf(value, comparisonType) >= 0;

        public static string ToTitleCase(this string str)
            => Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());

        public static string ToAlphaNumeric(this string str)
            => Regex.Replace(str, "[^a-zA-Z0-9 ]", String.Empty);

        public static string ToPhoneNumber(this string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                // Strip out non-digits
                str = Regex.Replace(str, @"\D", String.Empty);
                if (UInt64.TryParse(str, out UInt64 num))
                {
                    if (str.Length == 7)
                        return num.ToString("###-####");
                    if (str.Length == 10)
                        return num.ToString("(###) ###-####");
                    if (str.Length > 10)
                        return num.ToString("(###) ###-#### x" + new String('#', (str.Length - 10)));
                }
            }

            return str;
        }
    }
}
