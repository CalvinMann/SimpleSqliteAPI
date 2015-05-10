using SimpleSqlite.Columns;
using SimpleSqlite.Tables;

namespace SimpleSqlite.ForeignKeys
{
    public class ColumnForeignKey
    {
        public Table Table { get; private set; }
        public Column Column { get; private set; }

        public ColumnForeignKey(Table table, Column column)
        {
            Table = table;
            Column = column;
        }
    }
}
