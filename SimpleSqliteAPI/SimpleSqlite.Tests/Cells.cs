using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSqlite.Cells;
using SimpleSqlite.Columns;
using SimpleSqlite.Exceptions;

namespace SimpleSqlite.Tests
{
    [TestClass]
    public class Cells
    {
        [TestMethod]
        public void Update()
        {
            TestDatabase.CreateNew();
            var row = new Database(TestDatabase.Path).Tables["Table1"].Rows.First();
            row.Cells["Value1"].Value = "update";
            Assert.AreEqual("update", (string)row.Cells["Value1"].Value);

            var table = new Database(TestDatabase.Path).Tables["Table1"];
            Assert.IsNotNull(table.Rows.SingleOrDefault(r => (string)r.Cells["Value1"].Value == "update"));
        }

        [TestMethod]
        public void UpdateAfterAdd()
        {
            TestDatabase.CreateNew();
            var row = new Database(TestDatabase.Path).Tables["Table1"].Rows.Add(new Cell("Value1", "add"));
            Assert.AreEqual("add", (string)row.Cells["Value1"].Value);
            row.Cells["Value1"].Value = "update";

            row = new Database(TestDatabase.Path).Tables["Table1"].Rows.First();
            Assert.IsNotNull(row.Cells.Where(cell => (string)cell.Value == "update"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddCellToAttachedRow()
        {
            TestDatabase.CreateNew();
            var row = new Database(TestDatabase.Path).Tables["Table1"].Rows.First();
            row.Cells.Add("NewColumn", "test");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RemoveAttachedCell()
        {
            TestDatabase.CreateNew();
            var row = new Database(TestDatabase.Path).Tables["Table1"].Rows.First();
            row.Cells.Remove("Value1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCellColumnException))]
        public void AddWithOtherColumn()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);
            var otherColumn = database.Tables["Table2"].Columns.First();
            var cell = new Cell(otherColumn, "test");
            database.Tables["Table1"].Rows.Add(cell);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidTypeException))]
        public void AddNullToNotNullColumn()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);
            var table = database.Tables["Table1"];
            table.Rows.Add(new Cell("Value1", null), new Cell("Value2", 29.2));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTypeException))]
        public void AddToListColumn()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);
            var table = database.Tables["Table1"];
            table.Columns.Add("List", ColumnType.Text, ColumnQuantity.List);
            table.Rows.Add(new Cell("Value1", "test"), new Cell("Value2", 0.2), new Cell("List", "example"));
        }
    }
}
