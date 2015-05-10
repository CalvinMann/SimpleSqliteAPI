using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using SimpleSqlite.Base;
using SimpleSqlite.Cells;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Rows
{
    public class RowCollection : SimpleCollection<Row>
    {
        private const string RowsSql = @"SELECT {0} FROM ""{1}""";
        private const string ClearSql = @"DELETE FROM ""{0}""";

        public Table Table { get; private set; }

        internal RowCollection(Table table)
        {
            Table = table;
        }

        internal RowCollection(Table table, IEnumerable<Row> rows)
            : base(rows)
        {
            Table = table;
        }

        public Row this[Cell foreignKeyCell]
        {
            get
            {
                if (Table == null || foreignKeyCell.Column == null) return null;
                var foreignKey = foreignKeyCell.Column.ForeignKeys.FirstOrDefault(key => key.Table == Table);
                return foreignKey != null
                    ? this.FirstOrDefault(row => row.Cells[foreignKey.Column.Name].Value == foreignKeyCell.Value)
                    : null;
            }
        }

        public override void Add(Row row)
        {
            if (Contains(row)) return;
            if (row.Table != null && row.Table != Table)
                throw new AlreadyAttachedException(null, Resources.RowAlreadyAttached.FormatExt(Table.Name));
            if (Table.IsAttached)
                row.AddToTable(Table);
            base.Add(row);
        }

        public Row Add(IEnumerable<Cell> cells)
        {
            var attachedCell = cells.FirstOrDefault(cell => cell.Row != null);
            if (attachedCell != null)
                throw new AlreadyAttachedException(attachedCell.ColumnName,
                    Resources.CellAlreadyAttached.FormatExt(attachedCell.ColumnName));
            
            attachedCell = cells.FirstOrDefault(cell => cell.Column != null);
            if (attachedCell != null)
                throw new InvalidCellColumnException(attachedCell,
                    Resources.CellAlreadyAttachedToInvalidColumn.FormatExt(attachedCell.ColumnName));

            var row = new Row(Table, cells);
            Add(row);
            return row;
        }

        public Row Add(params Cell[] cells)
        {
            return Add((IEnumerable<Cell>)cells);
        }

        internal void Load(SQLiteConnection connection)
        {
            var columns = Table.Columns.ToArray(); // to preserve order
            var columnNames = String.Join(",", columns.Select(x => x.Name.EscapeIdentifier())) + ",_ROWID_";

            using (var command = new SQLiteCommand(String.Format(RowsSql, columnNames, Table.Name), connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var rowId = reader.GetInt64(columns.Length);
                    var row = new Row(Table, rowId);
                    for (var i = 0; i < columns.Length; i++)
                    {
                        row.Cells.AddExisting(new Cell(row, columns[i],
                            DbValue.FromDb(reader.GetValue(i), columns[i].Type, columns[i].Quantity)));
                    }
                    AddExisting(row);
                }
            }
        }

        internal void AddExisting(Row row)
        {
            base.Add(row);
        }

        public override bool Remove(Row row)
        {
            if (!Contains(row)) return false;
            row.RemoveFromTable();
            return base.Remove(row);
        }

        public override void Clear()
        {
            if (Table.IsAttached)
            {
                Table.Database.Connection.ExecuteNonQuery(ClearSql.FormatExt(Table.Name));
            }
            this.ForEach(row => row.Detach());
            base.Clear();
        }
    }
}
