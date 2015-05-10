using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleSqlite.Base
{
    /// <summary>
    /// A read-only collection which elements can be accessed by name.
    /// </summary>
    /// <typeparam name="T">Type of the collection elements.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ReadonlyCollectionDebuggerTypeProxy<>))]
    public class ReadonlyNamedCollection<T> : IReadOnlyCollection<T> where T : INamedObject
    {
        private readonly NamedCollection<T> _collection;

        public ReadonlyNamedCollection(IEnumerable<T> collection, bool ignoreCase = false)
        {
            _collection = new NamedCollection<T>(collection, ignoreCase);
        }

        public ReadonlyNamedCollection(NamedCollection<T> backgroundCollection, bool ignoreCase = false)
        {
            _collection = backgroundCollection;
        }

        public T this[string name]
        {
            get { return _collection[name]; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get { return _collection.Count; } }
    }

    internal sealed class ReadonlyCollectionDebuggerTypeProxy<T>
    {
        private readonly IReadOnlyCollection<T> _collection;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get { return _collection.ToArray(); }
        }

        public ReadonlyCollectionDebuggerTypeProxy(IReadOnlyCollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException();
            _collection = collection;
        }
    }
}
