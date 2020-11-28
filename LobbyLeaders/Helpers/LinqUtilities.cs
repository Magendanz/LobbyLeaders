using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LobbyLeaders.Helpers
{
    public static class LinqUtilities
    {
        public static TResult MostCommon<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            var groups = source.GroupBy(selector).OrderByDescending(i => i.Count());
            foreach (var group in groups)
                if (group.Key != null)
                    return group.Key;

            return default;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static Hashtable ToHashtable(this IEnumerable<KeyValuePair<string, object>> source)
        {
            var result = new Hashtable();
            foreach (var item in source)
                result.Add(item.Key, item.Value);

            return result;
        }
    }
}