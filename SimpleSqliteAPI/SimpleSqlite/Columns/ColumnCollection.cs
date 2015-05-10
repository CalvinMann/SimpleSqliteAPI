using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using SimpleSqlite.Base;
using SimpleSqlite.Cells;
using SimpleSqlite.Exceptions;
using SimpleSqlite.ForeignKeys;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Columns
{
    public class ColumnCollection : NamedCollection<Column>
    {
        private const string ColumnsSql = @"PRAGMA table_info(""{0}"")";

        public Table Table { get; private set; }

        public ReadonlyColumnCollection Required
        {
            get
            {
                var rowIdAlias = Table.PrimaryKey.SingleOrDefault(x => x.Type == ColumnType.Integer);
                return
                    new ReadonlyColumnCollection(
                        this.Where(col => !col.IsNullable && col.DefaultValue == null && col != rowIdAlias).ToList());
            }
        }

        internal ColumnCollection(Table table) 
            : base(true)
        {
            Table = table;
        }

        internal ColumnCollection(Table table, IEnumerable<Column> columns)
            : base(true)
        {
            Table = table;
            foreach (var column in columns) Add(column);
        }

        public override Column Add(Column column)
        {
            if (!Column.IsNameValid(column.Name))
                throw new InvalidNameException(column.Name, Resources.InvalidColumnName.FormatExt(column.Name));
            if (Contains(column.Name))
                throw new DuplicateException(column.Name, Resources.DuplicatedColumn.FormatExt(column.Name, Table.Name));
            if (column.Table == Table) return column;
            if (column.Table != null)
                throw new AlreadyAttachedException(column.Name,
                    Resources.ColumnAlreadyAttached.FormatExt(column.Name, Table.Name));

            column.AddToTable(Table);
            return AddBase(column);
        }

        public Column Add(string name, ColumnType type, ColumnQuantity quantity = ColumnQuantity.Single,
            bool isNullable = true, bool isPrimaryKey = false, DbValue defaultValue = null)
        {
            var column = new Column(name, type, quantity, isNullable, isPrimaryKey, defaultValue);
            return Add(column);
        }

        public Column AddForeignKey(string name, Column primaryColumn, bool isNullable = true, DbValue defaultValue = null)
        {
            if (primaryColumn == null)
                throw new ArgumentNullException(Resources.NullPrimaryColumn.FormatExt(name));
            if (primaryColumn.Table == null)
                throw new InvalidOperationException(Resources.CannotAddForeignKeyToColumnWithoutTable.FormatExt(name,
                    Table.Name, primaryColumn.Name));
            if (!primaryColumn.IsPrimaryKey)
                throw new InvalidOperationException(Resources.CannotAddForeignKeyToNotPrimary.FormatExt(name, Table.Name,
                    primaryColumn.Name, primaryColumn.Table.Name));

            var column = new Column(name, primaryColumn.Type, isNullable, defaultValue)
            {
                ForeignKeys = { new ColumnForeignKey(primaryColumn.Table, primaryColumn) }
            };
            return Add(column);
        }

        internal void Load(SQLiteConnection connection, IEnumerable<ListColumn> listColumns)
        {
            using (var command = new SQLiteCommand(String.Format(ColumnsSql, Table.Name), connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader.GetNullableString(1);
                    var listColumn = listColumns.FirstOrDefault(lc => lc.Column == name);
                    var type = (listColumn != null)
                        ? listColumn.Type
                        : ColumnTypeExtensions.Parse(reader.GetNullableString(2));
                    var quantity = (listColumn != null) ? ColumnQuantity.List : ColumnQuantity.Single;
                    var notNull = reader.GetBoolean(3);
                    var defValue = reader.GetNullableString(4);
                    var isKey = reader.GetBoolean(5);
                    var newColumn = new Column(Table, name, type, quantity, !notNull, isKey, defValue);
                    AddBase(newColumn);
                }
            }
        }

        internal void AddExisting(Column column)
        {
            AddBase(column);
        }

        private Column AddBase(Column column)
        {
            var result = base.Add(column);
            if (column.IsPrimaryKey && !Table.PrimaryKey.Contains(column))
                Table.PrimaryKey.Add(column);
            return result;
        }

        public override bool Remove(Column column)
        {
            if (!Contains(column)) return false;
            column.RemoveFromTable();
            return base.Remove(column);
        }

        public override bool Remove(string name)
        {
            return Remove(this[name]);
        }

        public override void Clear()
        {
            this.ForEach(col => Remove(col));
        }
    }
}
