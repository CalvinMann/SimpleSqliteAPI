using System;
using SimpleSqlite.Columns;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Exceptions
{
    public class TableChangeNotSupported : Exception
    {
        public Table Table { get; private set; }
        public Column Column { get; private set; }

        public TableChangeNotSupported(Table table, Column column, string message) 
            : base(message)
        {
            Table = table;
            Column = column;
        }
    }
}
