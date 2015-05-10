using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SimpleSqlite.Columns;
using SimpleSqlite.Helpers;
using SimpleSqlite.Tables;

namespace SimpleSqlite
{
    [DebuggerDisplay("{Connection.FilePath}")]
    public class Database
    {
        private const string TableListSql = @"SELECT ""name"" FROM ""sqlite_master"" WHERE ""type""='table';";

        private const string ListColumnsTable = "simplesqlite_listcolumns";
        private const string CreateListColumnsTableSql =
            @"CREATE TABLE """ + ListColumnsTable +
            @"""(""table"" TEXT NOT NULL, ""column"" TEXT NOT NULL, ""type"" INT NOT NULL);";
        private const string LoadListColumnsSql = @"SELECT ""Table"", ""Column"", ""Type"" FROM " + ListColumnsTable;

        private TableCollection _tables;

        internal Connection Connection { get; private set; }

        public TableCollection Tables
        {
            get { return _tables ?? (_tables = new TableCollection(this)); }
            private set { _tables = value; }
        }

        public Database(string filePath)
        {
            Connection = new Connection(filePath);
            Load();
        }

        private void Load()
        {
            if (!File.Exists(Connection.FilePath))
                SQLiteConnection.CreateFile(Connection.FilePath);

            List<ListColumn> listColumns = null;
            using (var connection = Connection.Connect())
            {
                using (var command = new SQLiteCommand(TableListSql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tableName = reader.GetString(0);
                        if (tableName.ToLower().StartsWith("sqlite_")) continue; // internal table
                        if (tableName.ToLower() == ListColumnsTable.ToLower())
                        {
                            listColumns = LoadListColumnTable(connection);
                        }
                        else
                        {
                            Tables.AddExisting(new Table(this, tableName));
                        }
                    }
                }
                if (listColumns == null)
                {
                    CreateListColumnTable(connection);
                    listColumns = new List<ListColumn>();
                }

                Tables.ForEach(table => table.Load(connection, listColumns.Where(lc => lc.Table == table.Name)));
                Tables.ForEach(table => table.LoadForeignKeys(connection));
            }
        }

        private List<ListColumn> LoadListColumnTable(SQLiteConnection connection)
        {
            var result = new List<ListColumn>();
            using (var command = new SQLiteCommand(LoadListColumnsSql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new ListColumn
                    {
                        Table = reader.GetString(0),
                        Column = reader.GetString(1),
                        Type = (ColumnType)reader.GetInt64(2)
                    });
                }
            }
            return result;
        }

        private void CreateListColumnTable(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(CreateListColumnsTableSql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
