using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSqlite.Columns;

namespace SimpleSqlite.Tests
{
    [TestClass]
    public class PrimaryKeys
    {
        [TestMethod]
        public void Load()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);

            var table1 = database.Tables["Table1"];
            Assert.AreEqual(1, table1.PrimaryKey.Count);
            Assert.AreSame(table1.Columns["Id"], table1.PrimaryKey.Single());

            var table2 = database.Tables["Table2"];
            Assert.AreEqual(1, table2.PrimaryKey.Count);
            Assert.AreSame(table2.Columns["Value3"], table2.PrimaryKey.Single());

            var table3 = database.Tables["Table3"];
            Assert.AreEqual(1, table3.PrimaryKey.Count);
            Assert.AreSame(table3.Columns["Value5"], table3.PrimaryKey.Single());
        }

        [TestMethod]
        public void Create()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            var column1 = new Column("id", ColumnType.Text, false, true);
            var column2 = new Column("value1", ColumnType.Integer);
            var table1 = database.Tables.Add("table1", column1, column2);
            Assert.AreEqual(1, table1.PrimaryKey.Count);
            Assert.IsTrue(table1.PrimaryKey.Contains(column1));
        }

        [TestMethod]
        public void CreateWithoutKey()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);

            var column1 = new Column("value1", ColumnType.Integer);
            var table1 = database.Tables.Add("table1", column1);
            Assert.AreEqual(0, table1.PrimaryKey.Count);
        }
    }
}
