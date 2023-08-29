using System.Collections.Generic;

namespace RouteNav.Avalonia.Internal;

internal static class HelperExtensions
{
    public static bool EqualsContent<TKey, TValue>(this IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB,
                                                   IEqualityComparer<TValue>? valueComparer = null)
    {
        if (dictA == dictB)
            return true;

        if (dictA.Count != dictB.Count)
            return false;

        valueComparer ??= EqualityComparer<TValue>.Default;
        foreach (var keyValuePair in dictA)
        {
            if (!dictB.TryGetValue(keyValuePair.Key, out var value))
                return false;

            if (!valueComparer.Equals(keyValuePair.Value, value))
                return false;
        }

        return true;
    }
}
