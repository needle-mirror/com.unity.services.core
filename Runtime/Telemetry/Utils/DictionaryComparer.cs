using System.Collections.Generic;

namespace Unity.Services.Core.Telemetry.Internal
{
    static class DictionaryComparer
    {
        public static bool Equals<TKey, TValue>(Dictionary<TKey, TValue> x, Dictionary<TKey, TValue> y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null)
                || ReferenceEquals(y, null)
                || x.Count != y.Count)
            {
                return false;
            }

            var valueComparer = EqualityComparer<TValue>.Default;
            foreach (var kvp in x)
            {
                if (!y.TryGetValue(kvp.Key, out var value2)
                    || !valueComparer.Equals(kvp.Value, value2))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
