using System.Collections.Generic;
using SimpleSqlite.Base;
using SimpleSqlite.Columns;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;

namespace SimpleSqlite.Tables
{
    public class TableCollection : NamedCollection<Table>
    {
        public Database Database { get; private set; }

        internal TableCollection(Database database) : base(true)
        {
            Database = database;
        }

        internal TableCollection(Database database, IEnumerable<Table> tables) : base(tables, true)
        {
            Database = database;
        }

        public override Table Add(Table table)
        {
            if (table.Name == null)
                throw new InvalidNameException(table.Name, Resources.InvalidTableName.FormatExt(table.Name));
            if (Contains(table.Name))
                throw new DuplicateException(table.Name, Resources.DuplicatedTable.FormatExt(table.Name));
            if (table.Database == Database) return table;
            if (table.IsAttached)
                throw new AlreadyAttachedException(table.Name, Resources.TableAlreadyAttached.FormatExt(table.Name));

            table.AddToDatabase(Database);
            return base.Add(table);
        }

        public Table Add(string name, IEnumerable<Column> columns)
        {
            var table = new Table(name, columns);
            Add(table);
            return table;
        }

        public Table Add(string name, params Column[] columns)
        {
            return Add(name, ((IEnumerable<Column>)columns));
        }

        internal void AddExisting(Table table)
        {
            base.Add(table);
        }

        public override bool Remove(Table table)
        {
            if (!Contains(table)) return false;
            table.RemoveFromDatabase();
            return base.Remove(table);
        }

        public override bool Remove(string name)
        {
            var table = this[name];
            if (table == null) return false;
            return Remove(table);
        }

        public override void Clear()
        {
            this.ForEach(table => Remove(table));
        }
    }
}
