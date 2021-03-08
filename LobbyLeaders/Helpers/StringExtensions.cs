using System;
using System.Collections;
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
        public static double KeywordMatch(this string strA, string strB)
            => strA.KeywordMatch(strB.Split());

        public static double KeywordMatch(this string str, string[] keywords)
            => str.Split().KeywordMatch(keywords);

        public static double KeywordMatch(this string[] words, string[] keywords)
        {
            var len = Math.Min(words.Length, keywords.Length);
            return len > 0 ? (double)words.Count(i => keywords.Contains(i)) / len : 0.0;
        }

        public static string[] SplitTrigrams(this string str)
        {
            if (String.IsNullOrWhiteSpace(str))
                return null;

            var result = new List<string>();
            str = " " + str.ToUpperInvariant().Trim() + " ";

            for (var i = 0; i < str.Length - 2; ++i)
                result.Add(str.Substring(i, 3));

            return result.ToArray();
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
