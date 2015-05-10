using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSqlite.Base;
using SimpleSqlite.Columns;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;
using SimpleSqlite.Rows;

namespace SimpleSqlite.Cells
{
    public class CellCollection : NamedCollection<Cell>
    {
        private Row _row;

        public Row Row
        {
            get { return _row; }
            private set
            {
                if (value == null) throw new ArgumentNullException("value");
                _row = value;
            }
        }

        internal CellCollection(Row row) : base(true)
        {
            Row = row;
        }

        internal CellCollection(Row row, IEnumerable<Cell> cells)
            : base(cells, true)
        {
            Row = row;
        }

        public override Cell Add(Cell cell)
        {
            if (cell.Row == Row) return cell;
            if (cell.Row != null)
                throw new AlreadyAttachedException(cell.ColumnName, Resources.CellAlreadyAttached.FormatExt(cell.ColumnName));
            if (Contains(cell.ColumnName))
                throw new DuplicateException(cell.ColumnName, Resources.DuplicatedCell.FormatExt(cell.ColumnName));
            if (Row.IsAttached)
                throw new InvalidOperationException(Resources.CannotAddToAttachedRow.FormatExt(cell.ColumnName));
            if (cell.Column == null && Row.Table != null)
                cell.LinkToColumn(Row.Table.Columns);

            cell.LinkToRow(Row);
            return base.Add(cell);
        }

        public void Add(string columnName, DbValue value)
        {
            Add(new Cell(columnName, value));
        }

        public void Add(Column column, DbValue value)
        {
            Add(new Cell(column, value));
        }

        public void AddForeignKey(Column column, Row primaryKeyRow)
        {
            if (!column.ForeignKeys.Any())
                throw new InvalidOperationException(Resources.ColumnIsNotForeignKey.FormatExt(column.Name));
            var foreignKey = column.ForeignKeys.First();
            if (primaryKeyRow.Table == null)
                throw new InvalidOperationException(Resources.PrimaryRowMustBeAttached.FormatExt(column.Name));
            if (foreignKey.Table != primaryKeyRow.Table)
                throw new InvalidOperationException(Resources.ColumnIsNotForeignKeyToPrimaryRow.FormatExt(column.Name,
                    foreignKey.Table.Name, primaryKeyRow.Table.Name));

            var value = primaryKeyRow.Cells[foreignKey.Column.Name].Value;
            Add(new Cell(column, value));
        }

        internal void AddExisting(Cell cell)
        {
            base.Add(cell);
        }

        internal void CheckColumns(IEnumerable<Column> columns)
        {
            this.ForEach(cell => cell.LinkToColumn(columns));
        }

        public override bool Remove(Cell cell)
        {
            if (!Contains(cell)) return false;
            if (Row.IsAttached)
                throw new InvalidOperationException(Resources.CannotRemoveAttachedCell.FormatExt(cell.ColumnName));
            cell.Unlink();
            return base.Remove(cell);
        }

        public override bool Remove(string name)
        {
            return Remove(this[name]);
        }
    }
}
