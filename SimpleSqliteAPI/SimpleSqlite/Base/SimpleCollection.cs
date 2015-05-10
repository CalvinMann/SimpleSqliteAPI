using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace SimpleSqlite.Base
{
    /// <summary>
    /// Represents a generic collection with an ability to add or remove items.
    /// </summary>
    /// <remarks>This is a simple implementation of <see cref="ICollection{T}"/> interface.  Strangely enough, .NET Framework
    /// does not contain such implementation, all existing implementation (even <see cref="Collection{T}"/>) also implements more
    /// specific interfaces, like <see cref="IList{T}"/> or <see cref="IDictionary{T,T}"/>.</remarks>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebuggerTypeProxy<>))]
    public class SimpleCollection<T> : ICollection<T>
    {
        private readonly List<T> _items;

        public SimpleCollection()
        {
            _items = new List<T>();
        }

        public SimpleCollection(IEnumerable<T> items)
        {
            _items = items.ToList();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(T item)
        {
            _items.Add(item);
        }

        public virtual void Clear()
        {
            _items.Clear();
        }

        public virtual bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public virtual bool Remove(T item)
        {
            return _items.Remove(item);
        }

        public virtual int Count { get { return _items.Count; } }

        public bool IsReadOnly { get { return ((ICollection<T>)_items).IsReadOnly; } }
    }
}
