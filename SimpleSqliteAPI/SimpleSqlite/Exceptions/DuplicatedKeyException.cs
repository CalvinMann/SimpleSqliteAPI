using System;
using System.Collections.Generic;
using SimpleSqlite.Cells;
using SimpleSqlite.Helpers;

namespace SimpleSqlite.Exceptions
{
    public class DuplicatedKeyException : Exception
    {
        public IEnumerable<Cell> PrimaryKey { get; private set; }

        public DuplicatedKeyException(IEnumerable<Cell> primaryKey)
        {
            PrimaryKey = primaryKey.Copy();
        }

        public DuplicatedKeyException(IEnumerable<Cell> primaryKey, string message)
            : base(message)
        {
            PrimaryKey = primaryKey.Copy();
        }
    }
}
