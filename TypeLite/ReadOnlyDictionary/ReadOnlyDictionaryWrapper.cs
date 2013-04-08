using System.Collections;
using System.Collections.Generic;

namespace TypeLite.ReadOnlyDictionary
{
    public class ReadOnlyDictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public ReadOnlyDictionaryWrapper(Dictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get { return _dictionary.Count; } }

        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
        }

        public IEnumerable<TKey> Keys { get { return _dictionary.Keys; } }
        public IEnumerable<TValue> Values { get { return _dictionary.Values; } }
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}
