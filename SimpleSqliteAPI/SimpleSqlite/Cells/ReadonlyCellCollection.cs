using System.Collections.Generic;
using SimpleSqlite.Base;

namespace SimpleSqlite.Cells
{
    public class ReadonlyCellCollection : ReadonlyNamedCollection<Cell>
    {
        public ReadonlyCellCollection(IEnumerable<Cell> collection)
            : base(collection, true)
        {
        }

        public ReadonlyCellCollection(NamedCollection<Cell> backgroundCollection)
            : base(backgroundCollection, true)
        {
        }
    }
}
