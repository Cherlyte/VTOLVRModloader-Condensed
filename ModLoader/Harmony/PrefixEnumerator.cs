using System;
using System.Collections;

namespace ModLoader.Harmony
{
    internal class PrefixEnumerator<T> : IEnumerable
    {
        private readonly IEnumerator _enumerator;
        private readonly Func<T, object> _prefixAction;
        private readonly T _type;
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public PrefixEnumerator(IEnumerator enumerator, Func<T, object> prefixAction, T type)
        {
            _enumerator = enumerator;
            _prefixAction = prefixAction;
            _type = type;
        }

        public IEnumerator GetEnumerator()
        {
            yield return _prefixAction(_type);
            while (_enumerator.MoveNext())
            {
                yield return null;
            }
        }
    }
}