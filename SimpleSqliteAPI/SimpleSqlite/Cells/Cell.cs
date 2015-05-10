using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SimpleSqlite.Base;
using SimpleSqlite.Columns;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;
using SimpleSqlite.Rows;

namespace SimpleSqlite.Cells
{
    [DebuggerDisplay("Column = {ColumnName}, Value = {Value.Value}")]
    public class Cell : INamedObject
    {
        private const string UpdateSql = @"UPDATE ""{0}"" SET ""{1}"" = {2} WHERE _ROWID_ = @RowId";

        private DbValue _value;

        public Row Row { get; private set; }
        public Column Column { get; private set; }
        public string ColumnName { get; private set; }

        public DbValue Value
        {
            get { return _value; }
            set { Update(value); }
        }

        public ColumnType? Type
        {
            get { return Column != null ? Column.Type : (ColumnType?)null; }
        }

        /// <summary>
        /// Gets a row from primary table if the cell contains a foreign key value. Null otherwise.
        /// </summary>
        public Row PrimaryRow
        {
            get
            {
                if (Column == null) return null;
                var foreignKey = Column.ForeignKeys.FirstOrDefault();
                if (foreignKey == null) return null;
                return foreignKey.Table.Rows.FirstOrDefault(row => row.Cells[foreignKey.Column.Name].Value == Value);
            }
        }

        /// <summary>
        /// Indicates whether the cell is linked to a physical cell in a database.
        /// </summary>
        public bool IsAttached { get { return Row != null && Row.IsAttached; } }

        public Cell(Column column)
        {
            if (column == null) throw new ArgumentNullException("column");
            Column = column;
            ColumnName = Column.Name;
        }

        public Cell(string columnName)
        {
            if (!Column.IsNameValid(columnName))
                throw new InvalidNameException(columnName, Resources.InvalidColumnName.FormatExt(columnName));
            ColumnName = columnName;
        }

        public Cell(Column column, DbValue value) : this(column)
        {
            Value = value;
        }

        public Cell(string columnName, DbValue value) : this(columnName)
        {
            Value = value;
        }

        internal Cell(Row row, Column column, DbValue value)
        {
            if (row == null) throw new ArgumentNullException("row");
            Row = row;
            Column = column;
            ColumnName = Column.Name;
            _value = value;
        }

        internal void LinkToRow(Row row)
        {
            Row = row;
        }

        internal void LinkToColumn(IEnumerable<Column> columns)
        {
            if (Column != null)
            {
                if (!columns.Contains(Column))
                    throw new InvalidCellColumnException(this,
                        Resources.CellAlreadyAttachedToInvalidColumn.FormatExt(ColumnName));
                ColumnName = Column.Name;
            }
            else
            {
                Column = columns.SingleOrDefault(x => x.Name == ColumnName);
                if (Column == null)
                    throw new ColumnNotFoundException(ColumnName, Resources.ColumnNotFound.FormatExt(ColumnName));
            }
            CheckType(Value);
        }

        private void CheckType(DbValue value)
        {
            if (value == null && Column.IsNullable == false)
                throw new InvalidTypeException(null, Resources.CannotBeNull.FormatExt(Column.Name));
            if (value != null)
            {
                if (Column.Quantity == ColumnQuantity.List && !value.IsListType)
                    throw new InvalidTypeException(value.Type,
                        Resources.ShouldBeListType.FormatExt(Column.Name, value.Type));
                if (Column.Quantity == ColumnQuantity.Single && value.IsListType)
                    throw new InvalidTypeException(value.Type,
                        Resources.ShouldBeSingleType.FormatExt(Column.Name, value.Type));
            }
        }

        internal void Unlink()
        {
            Row = null;
            Column = null;
        }

        private void Update(DbValue newValue)
        {
            if (_value == newValue) return;
            if (Column != null) CheckType(newValue);
            if (Row != null && Row.IsAttached)
            {
                var sql = UpdateSql.FormatExt(Row.Table.Name, ColumnName, ParamName);
                Row.Table.Database.Connection.ExecuteNonQuery(sql, new SQLiteParameter(ColumnName, newValue.ToDb()),
                    new SQLiteParameter("RowId", Row.RowId));
            }
            _value = newValue;
        }

        internal void SetExistingValue(DbValue value)
        {
            _value = value;
        }

        internal SQLiteParameter ToParameter()
        {
            return new SQLiteParameter(GetParamName(), Column.DbType) { Value = Value.ToDb() };
        }

        internal string ParamName
        {
            get { return String.Concat("@", GetParamName()); }
        }

        private string GetParamName()
        {
            var stringBuilder = new StringBuilder();
            ColumnName.Where(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                .ForEach(c => stringBuilder.Append(c));
            return stringBuilder.ToString();
        }

        string INamedObject.Name
        {
            get { return ColumnName; }
        }
    }
}
