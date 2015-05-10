using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using SimpleSqlite.Base;
using SimpleSqlite.Cells;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Rows
{
    [DebuggerTypeProxy(typeof(RowDebuggerTypeProxy))]
    public class Row
    {
        private const string InsertSql = @"INSERT INTO ""{0}"" ({1}) VALUES ({2})";
        private const string RemoveSql = @"DELETE FROM ""{0}"" WHERE _ROWID_ = @RowId";
        private const string LoadSql = @"SELECT {0} FROM ""{1}"" WHERE _ROWID_ = @RowId";

        private CellCollection _cells;

        public Table Table { get; private set; }
        public Int64? RowId { get; private set; }

        public CellCollection Cells
        {
            get { return _cells ?? (_cells = new CellCollection(this)); }
            private set { _cells = value; }
        }

        public ReadonlyCellCollection PrimaryKey
        {
            get
            {
                return new ReadonlyCellCollection(Cells.Where(cell => cell.Column != null && cell.Column.IsPrimaryKey));
            }
        }

        /// <summary>
        /// Indicates whether the row is linked to a physical row in a database.
        /// </summary>
        public bool IsAttached { get { return Table != null && Table.IsAttached; } }

        public Row() { }

        public Row(IEnumerable<Cell> cells)
        {
            Cells = new CellCollection(this, cells);
            Cells.ForEach(cell => cell.LinkToRow(this));
        }

        public Row(params Cell[] cells) : this((IEnumerable<Cell>)cells) { }

        internal Row(Table table, Int64? rowId = null)
        {
            RowId = rowId;
            Table = table;
        }

        internal Row(Table table, IEnumerable<Cell> cells, Int64? rowId = null) : this(table, rowId)
        {
            Cells = new CellCollection(this, cells);
            Cells.ForEach(cell => cell.LinkToRow(this));
        }

        internal Row(Table table, Int64? rowId = null, params Cell[] cells) : this(table, cells, rowId) { }

        internal void AddToTable(Table table)
        {
            using (var connection = table.Database.Connection.Connect())
            {
                AddToTable(table, connection);
            }
        }

        internal void AddToTable(Table table, SQLiteConnection connection)
        {
            Cells.CheckColumns(table.Columns);
            if (!Cells.Any())
                throw new InvalidOperationException(Resources.ZeroCellsInsert.FormatExt(table.Name));
            CheckMissedColumns(table);
            CheckPrimaryCells(table);

            var insert = InsertSql.FormatExt(table.Name,
                Cells.Select(x => x.Column.Name.EscapeIdentifier()).JoinExt(","),
                Cells.Select(x => x.ParamName).JoinExt(","));
            using (var command = new SQLiteCommand(insert, connection))
            {
                command.Parameters.AddRange(Cells.Select(x => x.ToParameter()).ToArray());
                command.ExecuteNonQuery();
            }
            RowId = connection.LastInsertRowId;
            Table = table;
            Reload(connection); // Reload in order to fill auto-filled cells (like autoincrement).
        }

        private void CheckMissedColumns(Table table)
        {
            var missed = table.Columns.Required.Where(col => !Cells.Contains(col.Name));
            if (missed.Any())
                throw new MissedCellsException(missed, Resources.RequiredCellsMissed.FormatExt(table.Name));
        }

        private void CheckPrimaryCells(Table table)
        {
            if (PrimaryKey.Count == 0) return;
            var curPrimaryKey = PrimaryKey.Select(cell => cell.Value);
            if (table.Rows.Any(row => row.PrimaryKey.Select(cell => cell.Value).CollectionEqual(curPrimaryKey)))
                throw new DuplicatedKeyException(PrimaryKey,
                    Resources.CannotAddDuplicatedPrimaryKey.FormatExt(table.Name));
        }

        private void Reload(SQLiteConnection connection)
        {
            var columns = Table.Columns.ToArray(); // to preserve order
            var columnNames = String.Join(",", columns.Select(x => x.Name.EscapeIdentifier()));
            using (var command = new SQLiteCommand(LoadSql.FormatExt(columnNames, Table.Name), connection))
            {
                command.Parameters.Add(new SQLiteParameter("@RowId", RowId));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (var i = 0; i < columns.Length; i++)
                        {
                            var value = DbValue.FromDb(reader.GetValue(i), columns[i].Type, columns[i].Quantity);
                            if (Cells[columns[i].Name] == null)
                                Cells.AddExisting(new Cell(this, columns[i], value));
                            else
                                Cells[columns[i].Name].SetExistingValue(value);
                        }
                    }
                }
            }
        }

        internal void RemoveFromTable()
        {
            if (IsAttached)
            {
                var sql = RemoveSql.FormatExt(Table.Name);
                Table.Database.Connection.ExecuteNonQuery(sql, new SQLiteParameter("RowId", RowId));
            }
            RowId = null;
            Table = null;
        }

        internal void Detach()
        {
            Table = null;
            RowId = null;
        }

        internal sealed class RowDebuggerTypeProxy
        {
            private readonly ICollection<Cell> _cells;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Cell[] Cells
            {
                get
                {
                    var array = new Cell[_cells.Count];
                    _cells.CopyTo(array, 0);
                    return array;
                }
            }

            public RowDebuggerTypeProxy(Row row)
            {
                if (row.Cells == null)
                    throw new ArgumentNullException();
                _cells = row.Cells;
            }
        } 
    }
}
