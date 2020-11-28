using System;
using System.Text.RegularExpressions;

namespace LobbyLeaders.Helpers
{
    public class KeywordParser
    {
        static readonly Regex filter = new Regex(@"[^\w\s&]+", RegexOptions.Compiled);
        static readonly Regex separator = new Regex(@"\s+", RegexOptions.Compiled);
        static readonly string[] empty = new string[0];

        public static string[] Parse(string str)
        {
            var text = filter.Replace(str.ToUpperInvariant(), String.Empty);
            return String.IsNullOrEmpty(text) ? empty : separator.Split(text);
        }
    }
}