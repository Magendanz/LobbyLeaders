using System;
using System.Text.RegularExpressions;

namespace LobbyLeaders.Helpers
{
    public class KeywordParser
    {
        static readonly Regex filter = new Regex(@"[^\w\s]+", RegexOptions.Compiled);
        static readonly Regex separator = new Regex(@"\s+", RegexOptions.Compiled);

        public static string[] Parse(string str)
            => separator.Split(filter.Replace(str.ToLower(), String.Empty));
    }
}