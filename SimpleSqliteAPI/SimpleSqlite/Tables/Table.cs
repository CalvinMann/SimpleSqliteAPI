using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using SimpleSqlite.Base;
using SimpleSqlite.Columns;
using SimpleSqlite.Exceptions;
using SimpleSqlite.ForeignKeys;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;
using SimpleSqlite.Rows;

namespace SimpleSqlite.Tables
{
    [DebuggerDisplay("{Name}")]
    public class Table : INamedObject
    {
        private const string CreateSql = @"CREATE TABLE ""{0}"" ({1}{2}{3});";
        private const string CreatePrimaryKeySql = @"PRIMARY KEY({0})";
        private const string CreateForeignKeySql = @"FOREIGN KEY(""{0}"") REFERENCES ""{1}""(""{2}"")";
        private const string RemoveSql = @"DROP TABLE ""{0}""";
        private const string RenameSql = @"ALTER TABLE ""{0}"" RENAME TO ""{1}""";
        private const string ForeignKeysSql = @"PRAGMA foreign_key_list(""{0}"")";

        private ColumnCollection _columns;
        private RowCollection _rows;
        private PrimaryKeyColumnCollection _primaryKeyColumn;
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (String.IsNullOrWhiteSpace(value) || value.Contains("\""))
                    throw new InvalidNameException(value, Resources.InvalidTableName.FormatExt(value));
                if (IsAttached) Rename(value);
                _name = value;
            }
        }

        public Database Database { get; private set; }

        public ColumnCollection Columns
        {
            get { return _columns ?? (_columns = new ColumnCollection(this)); }
            private set { _columns = value; }
        }

        public RowCollection Rows
        {
            get { return _rows ?? (_rows = new RowCollection(this)); }
            private set { _rows = value; }
        }

        public PrimaryKeyColumnCollection PrimaryKey
        {
            get { return _primaryKeyColumn ?? (_primaryKeyColumn = new PrimaryKeyColumnCollection(this)); }
            private set { _primaryKeyColumn = value; }
        }

        /// <summary>
        /// Indicates whether the table is linked to a physical table in a database.
        /// </summary>
        public bool IsAttached { get { return Database != null; } }

        public Table() { }

        public Table(string name)
        {
            Name = name;
        }

        public Table(string name, IEnumerable<Column> columns) 
            : this(name)
        {
            Columns = new ColumnCollection(this, columns);
            PrimaryKey = new PrimaryKeyColumnCollection(this, Columns.Where(x => x.IsPrimaryKey));
        }

        public Table(string name, params Column[] columns) 
            : this(name, (IEnumerable<Column>)columns) { }

        internal Table(Database database, string name) 
            : this(name)
        {
            Database = database;
        }

        internal Table(Database database, string name, IEnumerable<Column> columns)
            : this(name, columns)
        {
            Database = database;
        }

        internal Table(Database database, string name, params Column[] columns) 
            : this(database, name, (IEnumerable<Column>)columns) { }

        /// <summary>
        /// Creates the table in database.
        /// </summary>
        /// <remarks>Table must contain at least one column to be created (SQLite requirement).</remarks>
        internal void AddToDatabase(Database database)
        {
            if (String.IsNullOrEmpty(Name))
                throw new InvalidNameException(Name, Resources.InvalidTableName.FormatExt(Name));
            if (!Columns.Any())
                throw new InvalidOperationException(Resources.MustHaveColumns.FormatExt(Name));
            var invalidColumn = Columns.FirstOrDefault(col => !Column.IsNameValid(col.Name));
            if (invalidColumn != null)
                throw new InvalidNameException(invalidColumn.Name, Resources.InvalidColumnName.FormatExt(invalidColumn.Name));

            Database = database;
            using (var connection = Database.Connection.Connect())
            {
                var sql = CreateSql.FormatExt(Name, String.Join(",", Columns.Select(x => x.Definition)),
                    String.IsNullOrEmpty(PrimaryKeyDefinition) ? "" : "," + PrimaryKeyDefinition,
                    String.IsNullOrEmpty(ForeignKeysDefinitions) ? "" : "," + ForeignKeysDefinitions);
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }

                Columns.Where(c => c.Quantity == ColumnQuantity.List).ForEach(c => c.AddListColumn(connection));
                Rows.ForEach(row => row.AddToTable(this, connection));
            }
        }

        internal void Load(SQLiteConnection connection, IEnumerable<ListColumn> listColumns)
        {
            Columns.Load(connection, listColumns);
            Rows.Load(connection);
        }

        internal void LoadForeignKeys(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(String.Format(ForeignKeysSql, Name), connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var column = Columns[reader.GetString(3)];
                    var primaryTable = Database.Tables[reader.GetString(2)];
                    if (primaryTable == null || column == null) continue;
                    var primaryColumn = primaryTable.Columns[reader.GetString(4)];
                    if (primaryColumn == null) return;
                    
                    column.ForeignKeys.Add(new ColumnForeignKey(primaryTable, primaryColumn));
                }
            }
        }

        internal void RemoveFromDatabase()
        {
            if (!IsAttached)
                throw new InvalidOperationException("The table have not been added to a database yet.");
            var sql = RemoveSql.FormatExt(Name);
            Database.Connection.ExecuteNonQuery(sql);
            Database = null;
        }

        private void Rename(string newName)
        {
            var sql = RenameSql.FormatExt(Name, newName);
            Database.Connection.ExecuteNonQuery(sql);
        }

        private string PrimaryKeyDefinition
        {
            get
            {
                return PrimaryKey.Any()
                    ? CreatePrimaryKeySql.FormatExt(String.Join(",", PrimaryKey.Select(x => x.Name.EscapeIdentifier())))
                    : String.Empty;
            }
        }

        private string ForeignKeysDefinitions
        {
            get
            {
                return String.Join(",", Columns.Where(column => column.ForeignKeys.Any()).Select(
                    column => CreateForeignKeySql.FormatExt(column.Name, column.ForeignKeys.First().Table.Name,
                        column.ForeignKeys.First().Column.Name)));
            }
        }
    }
}
