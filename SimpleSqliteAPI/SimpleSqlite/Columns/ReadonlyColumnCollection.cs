using System.Collections.Generic;
using SimpleSqlite.Base;

namespace SimpleSqlite.Columns
{
    public class ReadonlyColumnCollection : ReadonlyNamedCollection<Column>
    {
        public ReadonlyColumnCollection(IEnumerable<Column> collection)
            : base(collection, true)
        {
        }

        public ReadonlyColumnCollection(NamedCollection<Column> backgroundCollection)
            : base(backgroundCollection, true)
        {
        }
    }
}
