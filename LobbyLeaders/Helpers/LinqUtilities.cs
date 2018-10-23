using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyLeaders.Helpers
{
    public static class LinqUtilities
    {
        public static TResult MostCommon<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.GroupBy(selector).OrderByDescending(i => i.Count()).First().Key;
        }
    }
}
