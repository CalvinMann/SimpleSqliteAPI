using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleSqlite.Base
{
    /// <summary>
    /// A collection which elements can be accessed by name.
    /// </summary>
    /// <typeparam name="T">Type of the collection elements.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebuggerTypeProxy<>))]
    public class NamedCollection<T> : ICollection<T>, ICollection where T : INamedObject
    {
        private readonly Dictionary<string, T> _dictionary;

        public NamedCollection(bool ignoreCase = false)
        {
            _dictionary = ignoreCase
                ? new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, T>();
        }

        public NamedCollection(IEnumerable<T> collection, bool ignoreCase = false)
        {
            _dictionary = ignoreCase
                ? collection.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase)
                : collection.ToDictionary(x => x.Name);
        }

        public T this[string name]
        {
            get { return _dictionary.ContainsKey(name) ? _dictionary[name] : default(T); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual T Add(T item)
        {
            _dictionary.Add(item.Name, item);
            return item;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public virtual void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(T item)
        {
            return _dictionary.ContainsValue(item);
        }

        public bool Contains(string name)
        {
            return _dictionary.ContainsKey(name);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _dictionary.Values.CopyTo(array, arrayIndex);
        }

        public virtual bool Remove(T item)
        {
            return _dictionary.Remove(item.Name);
        }

        public virtual bool Remove(string name)
        {
            return _dictionary.Remove(name);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_dictionary.Values).CopyTo(array, index);
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public object SyncRoot
        {
            get { return ((ICollection)_dictionary).SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return ((ICollection)_dictionary).IsSynchronized;  }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }

    internal sealed class CollectionDebuggerTypeProxy<T>
    {
        private readonly ICollection<T> _collection;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var array = new T[_collection.Count];
                _collection.CopyTo(array, 0);
                return array;
            }
        }

        public CollectionDebuggerTypeProxy(ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException();
            _collection = collection;
        }
    }
}
