using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSqlite.Cells;
using SimpleSqlite.Columns;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Tests
{
    [TestClass]
    public class Columns
    {
        [TestMethod]
        public void Load()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            TestColumn(table1, "Id", ColumnType.Integer, true, false);
            TestColumn(table1, "Value1", ColumnType.Text, false, false);
            TestColumn(table1, "Value2", ColumnType.Real, false, true);
            Assert.IsNull(table1.Columns["NonExistingColumn"]);
            Assert.IsNotNull(table1.Columns["value1"]); // casing

            var table2 = database.Tables["Table2"];
            Assert.IsNotNull(table2);
            Assert.AreEqual(2, table2.Columns.Count);
            TestColumn(table2, "Value3", ColumnType.Text, true, false);
            TestColumn(table2, "Value4", ColumnType.Text, false, true);

            var table3 = database.Tables["Table3"];
            Assert.IsNotNull(table3);
            Assert.AreEqual(5, table3.Columns.Count);
            TestColumn(table3, "Value5", ColumnType.Integer, true, false);
            TestColumn(table3, "Value6", ColumnType.Text, false, true);
            TestColumn(table3, "Value7", ColumnType.BLOB, false, false);
            TestColumn(table3, "Value8", ColumnType.Real, false, true);
            TestColumn(table3, "Value9", ColumnType.Numeric, false, false);
        }

        private void TestColumn(Table table, string name, ColumnType type, bool isPrimaryKey, bool isNullable)
        {
            var column = table.Columns[name];
            Assert.IsNotNull(column);
            Assert.AreEqual(name, column.Name);
            Assert.AreEqual(type, column.Type);
            Assert.AreEqual(isPrimaryKey, column.IsPrimaryKey);
            Assert.AreEqual(isNullable, column.IsNullable);
        }

        [TestMethod]
        public void Create()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            var table1 = new Table { Name = "table1" };
            table1.Columns.Add("value1", ColumnType.Numeric);
            var column2 = new Column("value2", ColumnType.Real);
            table1.Columns.Add(column2);
            database.Tables.Add(table1);

            database = new Database(TestDatabase.Path);
            var column1 = database.Tables["table1"].Columns["value1"];
            Assert.IsNotNull(column1);
            Assert.AreEqual(ColumnType.Numeric, column1.Type);
            column2 = database.Tables["table1"].Columns["value2"];
            Assert.IsNotNull(column2);
            Assert.AreEqual(ColumnType.Real, column2.Type);
        }

        [TestMethod]
        public void CreateWithDefaultValue()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            database.Tables.Add("table1",
                new Column("text", ColumnType.Text, true, false, "test"),
                new Column("int", ColumnType.Integer, true, false, 200),
                new Column("num", ColumnType.Numeric, true, false, 12.4M),
                new Column("real", ColumnType.Real, true, false, 99.2),
                new Column("blob", ColumnType.BLOB, true, false, new byte[] { 255, 13 }));

            database = new Database(TestDatabase.Path);
            var table = database.Tables["table1"];

            Assert.IsNotNull(table.Columns["text"]);
            Assert.AreEqual("test", table.Columns["text"].DefaultValue);

            Assert.IsNotNull(table.Columns["int"]);
            Assert.AreEqual((Int64)200, table.Columns["int"].DefaultValue);

            Assert.IsNotNull(table.Columns["num"]);
            Assert.AreEqual(12.4M, table.Columns["num"].DefaultValue);

            Assert.IsNotNull(table.Columns["real"]);
            Assert.AreEqual(99.2, table.Columns["real"].DefaultValue);

            Assert.IsNotNull(table.Columns["blob"]);
            Assert.IsTrue(new byte[] { 255, 13 }.SequenceEqual((byte[])table.Columns["blob"].DefaultValue));
        }

        [TestMethod]
        public void CreatePrimaryKey()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            var column1 = new Column("column1", ColumnType.Text, false, true);
            database.Tables.Add("table1", column1);

            database = new Database(TestDatabase.Path);
            column1 = database.Tables["table1"].Columns["column1"];
            Assert.IsNotNull(column1);
            Assert.AreEqual(true, column1.IsPrimaryKey);
        }

        [TestMethod]
        public void CreateInExistingTable()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            table1.Columns.Add("NewColumn", ColumnType.Integer);
            table1.Columns.Add("NotNullColumn", ColumnType.Real, ColumnQuantity.Single, false, false, 14.2);

            database = new Database(TestDatabase.Path);
            table1 = database.Tables["Table1"];

            Assert.IsNotNull(table1.Columns["NewColumn"]);
            Assert.AreEqual(ColumnType.Integer, table1.Columns["NewColumn"].Type);

            Assert.IsNotNull(table1.Columns["NotNullColumn"]);
            Assert.AreEqual(ColumnType.Real, table1.Columns["NotNullColumn"].Type);
            Assert.AreEqual(14.2, (double)table1.Columns["NotNullColumn"].DefaultValue);
        }

        [TestMethod]
        [ExpectedException(typeof(TableChangeNotSupported))]
        public void CreatePrimaryKeyInExistingTable()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            database.Tables["Table1"].Columns.Add("PKColumn", ColumnType.Integer,  ColumnQuantity.Single, false, true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateNotNullWithoutDefaultValueInExistingTable()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            database.Tables["Table1"].Columns.Add("NullColumn", ColumnType.Integer, ColumnQuantity.Single, false);
        }

        [TestMethod]
        public void CreateComplexName()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            var table1 = new Table { Name = "table1" };
            table1.Columns.Add("Complex1Column &Name_", ColumnType.Integer);
            table1.Rows.Add(new Cell("Complex1Column &Name_"));
            database.Tables.Add(table1);

            database = new Database(TestDatabase.Path);
            var column1 = database.Tables["table1"].Columns["Complex1Column &Name_"];
            Assert.IsNotNull(column1);
        }
    }
}
