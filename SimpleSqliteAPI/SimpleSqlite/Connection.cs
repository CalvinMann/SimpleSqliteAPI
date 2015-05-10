using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace SimpleSqlite
{
    internal class Connection
    {
        public string FilePath { get; private set; }

        public Connection(string filePath)
        {
            FilePath = filePath;
        }

        public SQLiteConnection Connect()
        {
            SQLiteConnection connection = null;
            try
            {
                var connString = new SQLiteConnectionStringBuilder { DataSource = FilePath, ForeignKeys = true }.ToString();
                connection = new SQLiteConnection(connString);
                connection.Open();
                return connection;
            }
            catch
            {
                if (connection != null) connection.Dispose();
                throw;
            }
        }

        public int ExecuteNonQuery(string sql, IEnumerable<SQLiteParameter> parameters)
        {
            using (var connection = Connect())
            using (var command = new SQLiteCommand(sql, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        public int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            return ExecuteNonQuery(sql, ((IEnumerable<SQLiteParameter>)parameters));
        }

        public Int64 ExecuteGetRowId(string sql, IEnumerable<SQLiteParameter> parameters)
        {
            using (var connection = Connect())
            using (var command = new SQLiteCommand(sql, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters.ToArray());
                command.ExecuteNonQuery();
                return connection.LastInsertRowId;
            }
        }

        public Int64 ExecuteGetRowId(string sql, params SQLiteParameter[] parameters)
        {
            return ExecuteGetRowId(sql, ((IEnumerable<SQLiteParameter>)parameters));
        }
    }
}
