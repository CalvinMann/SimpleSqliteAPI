using System;

namespace SimpleSqlite.Exceptions
{
    public class ColumnNotFoundException : Exception
    {
        public string ColumnName { get; private set; }

        public ColumnNotFoundException(string columnName)
        {
            ColumnName = columnName;
        }

        public ColumnNotFoundException(string columnName, string message) : base(message)
        {
            ColumnName = columnName;
        }
    }
}
