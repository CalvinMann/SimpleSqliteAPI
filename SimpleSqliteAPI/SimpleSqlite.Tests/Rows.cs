using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSqlite.Cells;
using SimpleSqlite.Columns;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Helpers;
using SimpleSqlite.Rows;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Tests
{
    [TestClass]
    public class Rows
    {
        [TestMethod]
        public void Load()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            Assert.AreEqual(3, table1.Rows.Count);
            Assert.IsTrue(table1.Rows.Count(x => 
                (string)x.Cells["Value1"].Value == "Test" &&
                (double)x.Cells["Value2"].Value == 15.5) == 1);
            Assert.IsTrue(table1.Rows.Count(x =>
                (string)x.Cells["Value1"].Value == "record 2" &&
                (double)x.Cells["Value2"].Value == 8) == 1);
            Assert.IsTrue(table1.Rows.Count(x =>
                (string)x.Cells["Value1"].Value == "  " &&
                x.Cells["Value2"].Value == null) == 1);
            Assert.IsTrue(table1.Rows.All(x => x.RowId != null));

            var table2 = database.Tables["Table2"];
            Assert.AreEqual(2, table2.Rows.Count);
            Assert.IsTrue(table2.Rows.Count(x =>
                (string)x.Cells["Value3"].Value == "key" &&
                x.Cells["Value4"].Value == null) == 1);
            Assert.IsTrue(table2.Rows.Count(x =>
                (string)x.Cells["Value3"].Value == "key2" &&
                (string)x.Cells["Value4"].Value == "value") == 1);
            Assert.IsTrue(table2.Rows.All(x => x.RowId != null));

            var table3 = database.Tables["Table3"];
            Assert.AreEqual(1, table3.Rows.Count);
            Assert.IsTrue(table3.Rows.Count(x =>
                (long)x.Cells["Value5"].Value == 15 &&
                (string)x.Cells["Value6"].Value == "test" &&
                ((byte[])x.Cells["Value7"].Value)[0] == 0xFD &&
                x.Cells["Value8"].Value == null &&
                (decimal)x.Cells["Value9"].Value == 10.5M) == 1);
            Assert.IsTrue(table3.Rows.All(x => x.RowId != null));

        }

        [TestMethod]
        public void Create()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table = database.Tables["Table3"];
            var row = new Row();
            row.Cells.Add("Value5", 20);
            row.Cells.Add("Value6", "example");
            row.Cells.Add("Value7", new Byte[] { 1, 2 });
            row.Cells.Add("Value8", 10.14123156921);
            row.Cells.Add("Value9", 5.6);
            table.Rows.Add(row);

            database = new Database(TestDatabase.Path);
            table = database.Tables["Table3"];
            Assert.AreEqual(2, table.Rows.Count);
            row = table.Rows.Single(x => (long)x.Cells["Value5"].Value == 20);
            Assert.IsNotNull(row);
            Assert.IsNotNull(row.RowId);
            Assert.IsTrue(row.Cells["Value5"].Type == ColumnType.Integer);
            Assert.AreEqual((long)20, row.Cells["Value5"].Value);
            Assert.IsTrue(row.Cells["Value6"].Type == ColumnType.Text);
            Assert.AreEqual("example", row.Cells["Value6"].Value);
            Assert.IsTrue(row.Cells["Value7"].Type == ColumnType.BLOB);
            Assert.AreEqual(1, ((byte[])row.Cells["Value7"].Value)[0]);
            Assert.AreEqual(2, ((byte[])row.Cells["Value7"].Value)[1]);
            Assert.IsTrue(row.Cells["Value8"].Type == ColumnType.Real);
            Assert.AreEqual(10.14123156921, row.Cells["Value8"].Value);
            Assert.IsTrue(row.Cells["Value9"].Type == ColumnType.Numeric);
            Assert.AreEqual(5.6M, row.Cells["Value9"].Value);
            Assert.IsNull(row.Cells["Value0"]);
        }

        [TestMethod]
        public void Remove()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table = database.Tables["Table3"];
            var row = table.Rows.Single(x => (string)x.Cells["Value6"].Value == "test");
            table.Rows.Remove(row);

            database = new Database(TestDatabase.Path);
            Assert.IsNull(database.Tables["Table3"].Rows.SingleOrDefault(x => (string)x.Cells["Value6"].Value == "test"));
        }

        [TestMethod]
        public void AddAndRemove()
        {
            TestDatabase.CreateNew();

            var table = new Database(TestDatabase.Path).Tables["Table1"];
            var row = new Row(new Cell("Value1", "example"), new Cell("Value2", 0.1));
            table.Rows.Add(row);
            table.Rows.Remove(row);
            Assert.IsFalse(row.IsAttached);

            table = new Database(TestDatabase.Path).Tables["Table1"];
            Assert.AreEqual(3, table.Rows.Count);
            Assert.IsNull(table.Rows.SingleOrDefault(x => (string)x.Cells["Value1"].Value == "test"));
        }

        [TestMethod]
        public void RemoveUnattached()
        {
            var table = new Table("Test", new Column("Column1", ColumnType.Text));
            var row = new Row(new Cell("Column1", "example"));
            table.Rows.Add(row);

            table.Rows.Remove(row);
            Assert.AreEqual(0, table.Rows.Count);
            Assert.IsFalse(row.IsAttached);
        }

        [TestMethod]
        public void Clear()
        {
            TestDatabase.CreateNew();

            var table = new Database(TestDatabase.Path).Tables["Table3"];
            var rows = table.Rows.Copy();
            table.Rows.Clear();

            Assert.AreEqual(0, table.Rows.Count);
            Assert.IsTrue(rows.All(row => !row.IsAttached && row.RowId == null && row.Table == null));

            table = new Database(TestDatabase.Path).Tables["Table3"];
            Assert.AreEqual(0, table.Rows.Count);
        }

        [TestMethod]
        public void ClearUnattached()
        {
            var table = new Table("Test", new Column("Column1", ColumnType.Text));
            table.Rows.Add(new Cell("Column1", "example"));
            table.Rows.Add(new Cell("Column1", "example2"));
            var rows = table.Rows.Copy();
            
            table.Rows.Clear();
            Assert.AreEqual(0, table.Rows.Count);
            Assert.IsTrue(rows.All(row => !row.IsAttached && row.RowId == null && row.Table == null));
        }

        [TestMethod]
        public void AddWithAutoField()
        {
            TestDatabase.CreateNew();

            var table = new Database(TestDatabase.Path).Tables["Table1"];
            var row = new Row(new Cell("Value1", "test"));
            table.Rows.Add(row);
            Assert.IsNotNull(row.Cells["Id"]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddWithoutCells()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);
            database.Tables.Add("Table1", new Column("id", ColumnType.Integer, false, true),
                new Column("value", ColumnType.Text));
            var row = new Row();
            database.Tables["Table1"].Rows.Add(row);
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicatedKeyException))]
        public void AddDuplicatedKey()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);
            var table = database.Tables.Add("Table1", new Column("id", ColumnType.Integer, false, true),
                new Column("id2", ColumnType.Text, false, true), new Column("value", ColumnType.Real));
            table.Rows.Add(new Cell("id", 5), new Cell("id2", "test"), new Cell("value", 18.3D));
            table.Rows.Add(new Cell("id", 5), new Cell("id2", "test"), new Cell("value", 2.2D));
        }
    }
}
