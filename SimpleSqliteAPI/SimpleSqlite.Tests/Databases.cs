using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleSqlite.Tests
{
    [TestClass]
    public class Databases
    {
        [TestMethod]
        public void Create()
        {
            TestDatabase.Delete();
            var database = new Database(TestDatabase.Path);
            Assert.IsTrue(File.Exists(TestDatabase.Path));
        }

        [TestMethod]
        public void Open()
        {
            TestDatabase.CreateNew();
            var database = new Database(TestDatabase.Path);
        }
    }
}
