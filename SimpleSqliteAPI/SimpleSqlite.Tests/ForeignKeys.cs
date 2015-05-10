using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSqlite.Columns;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Rows;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Tests
{
    [TestClass]
    public class ForeignKeys
    {
        [TestMethod]
        public void GetRowFromForeignKey()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);
            var foreignCell =
                database.Tables["Table4"].Rows.First(row => (string)row.Cells["Value"].Value == "value5").Cells["Table1Id"];

            Assert.IsNotNull(database.Tables["Table1"].Rows[foreignCell]);
            Assert.AreEqual("record 2", database.Tables["Table1"].Rows[foreignCell].Cells["Value1"].Value);

            Assert.IsNotNull(foreignCell.PrimaryRow);
            Assert.AreEqual("record 2", foreignCell.PrimaryRow.Cells["Value1"].Value);
        }

        [TestMethod]
        public void CreateForeignKey()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            var table5 = new Table("Table5");
            table5.Columns.AddForeignKey("Table1Id", table1.Columns["Id"]);
            database.Tables.Add(table5);

            database = new Database(TestDatabase.Path);
            table5 = database.Tables["Table5"];

            var table1Fk = table5.Columns["Table1Id"].ForeignKeys.FirstOrDefault();
            Assert.IsNotNull(table1Fk);
            Assert.AreSame(table1Fk.Table, database.Tables["Table1"]);
            Assert.AreSame(table1Fk.Column, database.Tables["Table1"].Columns["Id"]);
        }

        [TestMethod]
        public void CreateForeignKeyToNewTable()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            var table1 = new Table("Table1", new Column("Id", ColumnType.Integer, false, true),
                new Column("Value", ColumnType.Text));
            var table2 = new Table("Table2", new Column("Value2", ColumnType.Text));
            table2.Columns.AddForeignKey("Table1Id", table1.Columns["Id"]);

            database.Tables.Add(table1);
            database.Tables.Add(table2);

            database = new Database(TestDatabase.Path);
            var table1Fk = database.Tables["Table2"].Columns["Table1Id"].ForeignKeys.FirstOrDefault();
            Assert.IsNotNull(table1Fk);
        }

        [TestMethod]
        [ExpectedException(typeof(TableChangeNotSupported))]
        public void CreateForeignKeyToNotPrimaryKey()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table2 = database.Tables["Table2"];
            table2.Columns.AddForeignKey("Table1Id", database.Tables["Table1"].Columns["Id"]);
        }

        [TestMethod]
        public void AddRowWithForeignKey()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var primaryRow =
                database.Tables["Table1"].Rows.First(row => (string) row.Cells["Value1"].Value == "record 2");
            var table4 = database.Tables["Table4"];
            var newRow = new Row();
            newRow.Cells.AddForeignKey(table4.Columns["Table1Id"], primaryRow);
            newRow.Cells.Add("Value", "foreignTestRow");
            table4.Rows.Add(newRow);

            database = new Database(TestDatabase.Path);
            table4 = database.Tables["Table4"];
            var foreignRow = table4.Rows.First(row => (string) row.Cells["Value"].Value == "foreignTestRow");
            Assert.AreEqual(primaryRow.Cells["Id"].Value, foreignRow.Cells["Table1Id"].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddRowWithForeignKeyWrongColumn()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var primaryRow = database.Tables["Table1"].Rows.First();
            var newRow = new Row();
            newRow.Cells.AddForeignKey(database.Tables["Table4"].Columns["Value"], primaryRow);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddRowWithForeignKeyWrongTable()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var primaryRow = database.Tables["Table2"].Rows.First();
            var newRow = new Row();
            newRow.Cells.AddForeignKey(database.Tables["Table4"].Columns["Table1Id"], primaryRow);
        }
    }
}
