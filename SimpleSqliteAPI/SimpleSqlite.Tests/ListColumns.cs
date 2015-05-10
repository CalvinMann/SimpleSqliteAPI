using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSqlite.Cells;
using SimpleSqlite.Columns;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Tests
{
    [TestClass]
    public class ListColumns
    {
        [TestMethod]
        public void CreateAndLoad()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            database.Tables.Add(new Table("Table0", new Column("Value", ColumnType.Real)));
            database.Tables.Add(new Table("Table1",
                new Column("Id", ColumnType.Integer, false, true),
                new Column("ListColumn", ColumnType.Integer, ColumnQuantity.List)));

            database = new Database(TestDatabase.Path);
            var listColumn = database.Tables["Table1"].Columns["ListColumn"];
            Assert.AreEqual(ColumnQuantity.List, listColumn.Quantity);
            Assert.AreEqual(ColumnType.Integer, listColumn.Type);
            Assert.AreNotEqual(ColumnQuantity.List, database.Tables["Table1"].Columns["Id"].Quantity);
            Assert.AreNotEqual(ColumnQuantity.List, database.Tables["Table0"].Columns["Value"].Quantity);
        }

        [TestMethod]
        public void AddToExistingTable()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            database.Tables["Table1"].Columns.Add("ListColumn", ColumnType.Numeric, ColumnQuantity.List);

            database = new Database(TestDatabase.Path);
            var listColumn = database.Tables["Table1"].Columns["ListColumn"];
            Assert.AreEqual(ColumnQuantity.List, listColumn.Quantity);
            Assert.AreEqual(ColumnType.Numeric, listColumn.Type);
        }

        [TestMethod]
        public void AddAndLoadValue()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            var table = database.Tables.Add("Table",
                new Column("IntList", ColumnType.Integer, ColumnQuantity.List),
                new Column("BinList", ColumnType.BLOB, ColumnQuantity.List),
                new Column("NumList", ColumnType.Numeric, ColumnQuantity.List),
                new Column("RealList", ColumnType.Real, ColumnQuantity.List),
                new Column("TextList", ColumnType.Text, ColumnQuantity.List));

            var intList = new long[] { 5, 2, 1, 8 };
            var binList = new[] { new byte[] { 255, 0, 12, 4 }, new byte[] { 92, 14, 3 }, new byte[9] };
            var numList = new[] { 5.2M, 2.0M, 11.1M, 8.98721M };
            var realList = new[] { 18.288, 6.914, 5.001 };
            var textList = new[] { "example", "test", "another sample string" };

            table.Rows.Add(new Cell("IntList", intList), new Cell("BinList", binList), new Cell("NumList", numList),
                new Cell("RealList", realList), new Cell("TextList", textList));

            var row = new Database(TestDatabase.Path).Tables["Table"].Rows.Single();
            CollectionAssert.AreEqual(intList, (long[])row.Cells["IntList"].Value);
            var binListResult = (byte[][])row.Cells["BinList"].Value;
            Assert.AreEqual(binList.Length, binListResult.Length);
            for (var i = 0; i < binList.Length; i++)
            {
                CollectionAssert.AreEqual(binList[i], binListResult[i]);
            }
            CollectionAssert.AreEqual(numList, (decimal[])row.Cells["NumList"].Value);
            CollectionAssert.AreEqual(realList, (double[])row.Cells["RealList"].Value);
            CollectionAssert.AreEqual(textList, (string[])row.Cells["TextList"].Value);
        }

        [TestMethod]
        public void UpdateValue()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);
            var table = database.Tables.Add("Table", new Column("IntList", ColumnType.Integer, ColumnQuantity.List));
            table.Rows.Add(new Cell("IntList", new long[] { 5, 2, 1, 8 }));

            var row = new Database(TestDatabase.Path).Tables["Table"].Rows.Single();
            row.Cells["IntList"].Value = new long[] { 198142, 15, -40 };

            row = new Database(TestDatabase.Path).Tables["Table"].Rows.Single();
            CollectionAssert.AreEqual(new long[] { 198142, 15, -40 }, (long[])row.Cells["IntList"].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTypeException))]
        public void WrongValue()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);
            var table = database.Tables.Add("Table", new Column("BinList", ColumnType.BLOB, ColumnQuantity.List));
            table.Rows.Add(new Cell("BinList", 15));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTypeException))]
        public void UpdateWithWrongValue()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);
            var table = database.Tables.Add("Table", new Column("List", ColumnType.Real, ColumnQuantity.List));
            table.Rows.Add(new Cell("List", new [] { 90.002, 84.1 }));

            var row = new Database(TestDatabase.Path).Tables["Table"].Rows.First();
            row.Cells["List"].Value = new byte[9];
        }
    }
}
