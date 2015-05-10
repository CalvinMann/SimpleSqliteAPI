using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSqlite.Cells;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Rows;

namespace SimpleSqlite.Tests
{
    [TestClass]
    public class Querying
    {
        [TestMethod]
        public void WhereEqual()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            table1.Rows.Add(new Cell("Value1", "record 2"), new Cell("Value2", null));

            var whereRows = table1.Rows.Where("Value1", Comparison.Equal, "record 2");
            Assert.AreEqual(2, whereRows.Count());
            Assert.AreEqual("record 2", whereRows.First().Cells["Value1"].Value);
            Assert.AreEqual("record 2", whereRows.Last().Cells["Value1"].Value);
            Assert.AreEqual(8D, whereRows.First().Cells["Value2"].Value);
            Assert.AreEqual(null, whereRows.Last().Cells["Value2"].Value);
        }

        [TestMethod]
        public void WhereGreaterThan()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);
            
            var table1 = database.Tables["Table1"];
            var whereRows = table1.Rows.Where("Value2", Comparison.GreaterThan, 10);

            Assert.AreEqual(1, whereRows.Count());
            Assert.AreEqual(15.5, whereRows.First().Cells["Value2"].Value);
        }

        [TestMethod]
        public void WhereNull()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table2 = database.Tables["Table2"];
            var whereRows = table2.Rows.Where("Value4", Comparison.Equal, null);

            Assert.AreEqual(1, whereRows.Count());
            Assert.AreEqual(null, whereRows.First().Cells["Value4"].Value);
        }

        [TestMethod]
        public void WhereWrongType()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            var whereRows = table1.Rows.Where("Value1", Comparison.Equal, 20);

            Assert.AreEqual(0, whereRows.Count());
        }

        [TestMethod]
        public void WhereMultiCondition()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            table1.Rows.Add(new Cell("Value1", "test"), new Cell("Value2", 1.5));

            var whereRows = table1.Rows.Where(new Condition("Value2", Comparison.GreaterThanOrEqual, 8),
                new Condition("Value2", Comparison.LessThan, 13.5));
            Assert.AreEqual(1, whereRows.Count());
            Assert.AreEqual(8D, whereRows.First().Cells["Value2"].Value);
        }

        [TestMethod]
        public void Like()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            var likeRows = table1.Rows.Like("Value1", "%e[sc]%");
            Assert.AreEqual(2, likeRows.Count());
            Assert.AreEqual("Test", likeRows.First().Cells["Value1"].Value);
            Assert.AreEqual("record 2", likeRows.Last().Cells["Value1"].Value);
        }

        [TestMethod]
        public void LikeWrongType()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            var likeRows = table1.Rows.Like("Value2", "%1%");
            Assert.AreEqual(0, likeRows.Count());
        }

        [TestMethod]
        public void Order()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            table1.Rows.Add(new Cell("Value1", "xyz"), new Cell("Value2", 8D));

            var ordered = table1.Rows.Order(new Order("Value2", true), new Order("Value1", false)).ToArray();
            Assert.AreEqual("  ", ordered[0].Cells["Value1"].Value);
            Assert.AreEqual("xyz", ordered[1].Cells["Value1"].Value);
            Assert.AreEqual("record 2", ordered[2].Cells["Value1"].Value);
            Assert.AreEqual("Test", ordered[3].Cells["Value1"].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void OrderWrongType()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table3 = database.Tables["Table3"];
            table3.Rows.Add(new Cell("Value5", 22), new Cell("Value7", new byte[] { 90, 12, 3 }),
                new Cell("Value9", 12.0));
            table3.Rows.Order("Value7").ToList();
        }

        [TestMethod]
        public void Max()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            var max = table1.Rows.Max("Value2");
            Assert.AreEqual(15.5D, max);
        }

        [TestMethod]
        public void Min()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table = database.Tables["Table3"];
            table.Rows.Add(new Cell("Value7", new byte[] { }), new Cell("Value8", 42D), new Cell("Value9", 0));
            table.Rows.Add(new Cell("Value7", new byte[] { }), new Cell("Value8", 22.11D), new Cell("Value9", 0));

            var min = table.Rows.Min("Value8");
            Assert.AreEqual(22.11D, min);
        }

        [TestMethod]
        public void Sum()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table = database.Tables["Table1"];
            var sum = table.Rows.Sum("Value2");
            Assert.AreEqual(15.5D + 8D, sum);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTypeException))]
        public void SumWrongType()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table = database.Tables["Table3"];
            table.Rows.Add(new Cell("Value7", new byte[] { 5, 2, 4 }), new Cell("Value8", 0D), new Cell("Value9", 0));
            table.Rows.Add(new Cell("Value7", new byte[] { 8, 1 }), new Cell("Value8", 0D), new Cell("Value9", 0));

            table.Rows.Sum("Value7");
        }

        [TestMethod]
        public void SumNull()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table = database.Tables["Table3"];
            var sum = table.Rows.Sum("Value8");
            Assert.AreEqual(null, sum);
        }

        [TestMethod]
        public void Average()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table = database.Tables["Table1"];
            var avg = table.Rows.Average("Value2");
            Assert.AreEqual((15.5D + 8D) / 2D, avg);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTypeException))]
        public void AverageWrongType()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table = database.Tables["Table1"];
            table.Rows.Average("Value1");
        }
    }
}
