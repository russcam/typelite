using System.Collections.Generic;

namespace TypeLite.ReadOnlyDictionary
{
    public interface IReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        int Count { get; }

        TValue this[TKey key] { get; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }

        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);
    }
}
