using System;
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
    }
}