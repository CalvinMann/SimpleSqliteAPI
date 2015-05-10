using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SimpleSqlite.Columns;

namespace SimpleSqlite.Exceptions
{
    public class MissedCellsException : Exception
    {
        public ReadOnlyCollection<Column> Columns { get; private set; }

        public MissedCellsException(IEnumerable<Column> columns, string message) : base(message)
        {
            Columns = new ReadOnlyCollection<Column>(columns.ToList());
        }
    }
}
