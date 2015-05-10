using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Resources;

namespace SimpleSqlite.Tests
{
    public static class TestDatabase
    {
        public const string Path = "TestDatabase.sqlite";
        public const string CreateTablesScriptPath = "SimpleSqlite.Tests.SQL.CreateTables.sql";

        public static void CreateNew()
        {
            Delete();
            Create();
        }

        public static void EnsureExists()
        {
            if (!File.Exists(Path)) Create();
        }

        public static void Delete()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }

        private static void Create()
        {
            SQLiteConnection.CreateFile(Path);

            // Load database table creation script from resources
            var assembly = Assembly.GetExecutingAssembly();
            string sqlCreateTables;
            var stream = assembly.GetManifestResourceStream(CreateTablesScriptPath);
            if (stream == null) throw new MissingManifestResourceException("Cannot find database SQL script.]");
            using (var reader = new StreamReader(stream))
            {
                sqlCreateTables = reader.ReadToEnd();
            }

            // Create database tables
            var connString = new SQLiteConnectionStringBuilder { DataSource = Path }.ToString();
            using (var connection = new SQLiteConnection(connString))
            using (var command = new SQLiteCommand(sqlCreateTables, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
