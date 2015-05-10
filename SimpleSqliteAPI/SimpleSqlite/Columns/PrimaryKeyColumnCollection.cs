using System;
using System.Collections.Generic;
using SimpleSqlite.Base;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Columns
{
    public class PrimaryKeyColumnCollection : NamedCollection<Column>
    {
        public Table Table { get; private set; }

        internal PrimaryKeyColumnCollection(Table table) 
            : base(true)
        {
            Table = table;
        }

        internal PrimaryKeyColumnCollection(Table table, IEnumerable<Column> columns) 
            : base(columns, true)
        {
            Table = table;
        }

        public override Column Add(Column column)
        {
            if (column.Table != Table)
                throw new InvalidOperationException(Resources.CannotAddColumnFromOtherTableToPK.FormatExt(column.Name,
                    Table.Name));
            column.IsPrimaryKey = true;
            if (!Contains(column)) return base.Add(column);
            return column;
        }

        public override bool Remove(Column item)
        {
            if (!Contains(item)) return false;
            item.IsPrimaryKey = false;
            return base.Remove(item);
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
