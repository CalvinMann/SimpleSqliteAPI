using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSqlite.Cells;
using SimpleSqlite.Columns;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Rows;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Tests
{
    [TestClass]
    public class Tables
    {
        [TestMethod]
        public void Load()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            Assert.AreEqual(4, database.Tables.Count);
            Assert.IsTrue(
                new[] { "Table1", "Table2", "Table3", "Table4" }.All(
                    name => database.Tables.Count(table => table.Name == name) == 1));
        }

        [TestMethod]
        public void Add()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            var column1 = new Column("column1", ColumnType.Text);
            var table1 = database.Tables.Add("table1", column1);
            Assert.AreEqual("table1", table1.Name);

            var table2 = new Table { Name = "table2" };
            table2.Columns.Add("column2", ColumnType.Numeric);
            database.Tables.Add(table2);

            database = new Database(TestDatabase.Path);
            Assert.AreEqual(2, database.Tables.Count);
            Assert.IsNotNull(database.Tables["table1"]);
            Assert.IsNotNull(database.Tables["table2"]);
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddWithoutColumns()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);
            var table = database.Tables.Add("TestTable");
            database.Tables.Add(table);
        }

        [TestMethod]
        public void AddWithRows()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            var table1 = new Table("table1");
            table1.Columns.Add("column1", ColumnType.Text);
            var column2 = new Column("column2", ColumnType.Integer);
            table1.Columns.Add(column2);

            var row1 = new Row(new Cell("column1", "test"), new Cell("column2", 15));
            table1.Rows.Add(row1);
            table1.Rows.Add(new Cell("column2", 5), new Cell("column1", "example"));

            database.Tables.Add(table1);

            database = new Database(TestDatabase.Path);
            table1 = database.Tables["table1"];
            Assert.IsNotNull(table1);
            Assert.AreEqual(2, table1.Columns.Count);
            Assert.AreEqual(2, table1.Rows.Count);
        }

        [TestMethod]
        public void AddComplexName()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            var table1 = new Table("Complex Table.Name*1");
            table1.Columns.Add("column1", ColumnType.Text);
            table1.Rows.Add(new Cell("column1", "test"));
            database.Tables.Add(table1);

            database = new Database(TestDatabase.Path);
            table1 = database.Tables["Complex Table.Name*1"];
            Assert.IsNotNull(table1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void AddWrongName()
        {
            new Database(TestDatabase.Path).Tables.Add("", new Column("column1", ColumnType.Integer));
        }

        [TestMethod]
        public void Remove()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table3 = database.Tables["Table3"];
            Assert.IsTrue(database.Tables.Remove(table3));
            Assert.IsTrue(database.Tables.Remove("Table4"));

            database = new Database(TestDatabase.Path);
            Assert.IsNull(database.Tables["Table3"]);
            Assert.IsNull(database.Tables["Table4"]);
        }

        [TestMethod]
        public void RemoveNonExisting()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var tablesCount = database.Tables.Count;
            var result = database.Tables.Remove("NonExistingTable");
            Assert.IsFalse(result);

            database = new Database(TestDatabase.Path);
            Assert.AreEqual(tablesCount, database.Tables.Count);
        }

        [TestMethod]
        public void Rename()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table2 = database.Tables["Table2"];
            table2.Name = "RenamedTable";

            database = new Database(TestDatabase.Path);
            Assert.IsNotNull(database.Tables["RenamedTable"]);
            Assert.IsNull(database.Tables["Table2"]);
        }

        [TestMethod]
        public void ForeignKeysLoad()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table4 = database.Tables["Table4"];
            Assert.AreEqual(1, table4.Columns["Table1Id"].ForeignKeys.Count);
            var key = table4.Columns["Table1Id"].ForeignKeys.First();
            Assert.AreSame(database.Tables["Table1"], key.Table);
            Assert.AreSame(database.Tables["Table1"].Columns["Id"], key.Column);
        }
    }
}
