using System;

namespace SimpleSqlite.Exceptions
{
    public class TableNotFoundException : Exception
    {
        public string TableName { get; private set; }

        public TableNotFoundException(string tableName, string message)
            : base(message)
        {
            TableName = tableName;
        }
    }
}
