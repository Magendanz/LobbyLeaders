using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace LobbyLeaders.Helpers
{
    public class KeywordParser
    {
        static readonly Regex filter = new Regex(@"[^\w\s&]+", RegexOptions.Compiled);
        static readonly Regex separator = new Regex(@"\s+", RegexOptions.Compiled);
        static readonly string[] empty = new string[0];

        KeyValuePair<string, string>[] dictionary;

        public KeywordParser(string path = null)
        {
            // Load optional dictionary with abbreviations, aliases and nicknames
            dictionary = String.IsNullOrEmpty(path) ? null : File.ReadAllLines(path)
                .Select(i => i.Split(','))
                .Select(j => new KeyValuePair<string, string>(j[0], j[1]))
                .ToArray();
        }

        public string[] Parse(string str)
        {
            // Cast to lowercase and decode any HTML entities (e.g. &AMP;)
            str = WebUtility.HtmlDecode(str.ToLowerInvariant());

            // Filter out uninteresting characters
            str = filter.Replace(str, String.Empty);

            if (dictionary != null)
            {
                // Replace abbreviations, aliases and nicknames with canonical names
                foreach (var entry in dictionary)
                    str = Regex.Replace(str, $@"\b{entry.Key}\b", entry.Value);
            }

            // Return result split into words
            return String.IsNullOrEmpty(str) ? empty : separator.Split(str).Distinct().ToArray();
        }
    }
}