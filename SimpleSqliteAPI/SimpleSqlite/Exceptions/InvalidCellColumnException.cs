using System;
using SimpleSqlite.Cells;

namespace SimpleSqlite.Exceptions
{
    public class InvalidCellColumnException : Exception
    {
        public Cell Cell { get; set; }

        public InvalidCellColumnException(Cell cell)
        {
            Cell = cell;
        }

        public InvalidCellColumnException(Cell cell, string message) : base(message)
        {
            Cell = cell;
        }
    }
}
