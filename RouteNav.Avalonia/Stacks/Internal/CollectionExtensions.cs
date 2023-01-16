using System.Collections.Generic;

namespace NSE.RouteNav.Stacks.Internal;

internal static class CollectionExtensions
{
    public static void Set<TK, TV>(this Dictionary<TK, TV> dictionary, TK key, TV value)
        where TK : notnull
    {
        if (!dictionary.ContainsKey(key))
            dictionary.Add(key, value);
        else
            dictionary[key] = value;
    }
}
