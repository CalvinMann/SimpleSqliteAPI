using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSqlite.Helpers;

namespace SimpleSqlite.Tests
{
    [TestClass]
    public class Like
    {
        [TestMethod]
        public void Test()
        {
            // http://stackoverflow.com/a/8583383/860913
            TestSqlLikePattern(true, "%", "");
            TestSqlLikePattern(true, "%", " ");
            TestSqlLikePattern(true, "%", "asdfa asdf asdf");
            TestSqlLikePattern(true, "%", "%");
            TestSqlLikePattern(false, "_", "");
            TestSqlLikePattern(true, "_", " ");
            TestSqlLikePattern(true, "_", "4");
            TestSqlLikePattern(true, "_", "C");
            TestSqlLikePattern(false, "_", "CX");
            TestSqlLikePattern(false, "[ABCD]", "");
            TestSqlLikePattern(true, "[ABCD]", "A");
            TestSqlLikePattern(true, "[ABCD]", "b");
            TestSqlLikePattern(false, "[ABCD]", "X");
            TestSqlLikePattern(false, "[ABCD]", "AB");
            TestSqlLikePattern(true, "[B-D]", "C");
            TestSqlLikePattern(true, "[B-D]", "D");
            TestSqlLikePattern(false, "[B-D]", "A");
            TestSqlLikePattern(false, "[^B-D]", "C");
            TestSqlLikePattern(false, "[^B-D]", "D");
            TestSqlLikePattern(true, "[^B-D]", "A");
            TestSqlLikePattern(true, "%TEST[ABCD]XXX", "lolTESTBXXX");
            TestSqlLikePattern(false, "%TEST[ABCD]XXX", "lolTESTZXXX");
            TestSqlLikePattern(false, "%TEST[^ABCD]XXX", "lolTESTBXXX");
            TestSqlLikePattern(true, "%TEST[^ABCD]XXX", "lolTESTZXXX");
            TestSqlLikePattern(true, "%TEST[B-D]XXX", "lolTESTBXXX");
            TestSqlLikePattern(true, "%TEST[^B-D]XXX", "lolTESTZXXX");
            TestSqlLikePattern(true, "%Stuff.txt", "Stuff.txt");
            TestSqlLikePattern(true, "%Stuff.txt", "MagicStuff.txt");
            TestSqlLikePattern(false, "%Stuff.txt", "MagicStuff.txt.img");
            TestSqlLikePattern(false, "%Stuff.txt", "Stuff.txt.img");
            TestSqlLikePattern(false, "%Stuff.txt", "MagicStuff001.txt.img");
            TestSqlLikePattern(true, "Stuff.txt%", "Stuff.txt");
            TestSqlLikePattern(false, "Stuff.txt%", "MagicStuff.txt");
            TestSqlLikePattern(false, "Stuff.txt%", "MagicStuff.txt.img");
            TestSqlLikePattern(true, "Stuff.txt%", "Stuff.txt.img");
            TestSqlLikePattern(false, "Stuff.txt%", "MagicStuff001.txt.img");
            TestSqlLikePattern(true, "%Stuff.txt%", "Stuff.txt");
            TestSqlLikePattern(true, "%Stuff.txt%", "MagicStuff.txt");
            TestSqlLikePattern(true, "%Stuff.txt%", "MagicStuff.txt.img");
            TestSqlLikePattern(true, "%Stuff.txt%", "Stuff.txt.img");
            TestSqlLikePattern(false, "%Stuff.txt%", "MagicStuff001.txt.img");
            TestSqlLikePattern(true, "%Stuff%.txt", "Stuff.txt");
            TestSqlLikePattern(true, "%Stuff%.txt", "MagicStuff.txt");
            TestSqlLikePattern(false, "%Stuff%.txt", "MagicStuff.txt.img");
            TestSqlLikePattern(false, "%Stuff%.txt", "Stuff.txt.img");
            TestSqlLikePattern(false, "%Stuff%.txt", "MagicStuff001.txt.img");
            TestSqlLikePattern(true, "%Stuff%.txt", "MagicStuff001.txt");
            TestSqlLikePattern(true, "Stuff%.txt%", "Stuff.txt");
            TestSqlLikePattern(false, "Stuff%.txt%", "MagicStuff.txt");
            TestSqlLikePattern(false, "Stuff%.txt%", "MagicStuff.txt.img");
            TestSqlLikePattern(true, "Stuff%.txt%", "Stuff.txt.img");
            TestSqlLikePattern(false, "Stuff%.txt%", "MagicStuff001.txt.img");
            TestSqlLikePattern(false, "Stuff%.txt%", "MagicStuff001.txt");
            TestSqlLikePattern(true, "%Stuff%.txt%", "Stuff.txt");
            TestSqlLikePattern(true, "%Stuff%.txt%", "MagicStuff.txt");
            TestSqlLikePattern(true, "%Stuff%.txt%", "MagicStuff.txt.img");
            TestSqlLikePattern(true, "%Stuff%.txt%", "Stuff.txt.img");
            TestSqlLikePattern(true, "%Stuff%.txt%", "MagicStuff001.txt.img");
            TestSqlLikePattern(true, "%Stuff%.txt%", "MagicStuff001.txt");
            TestSqlLikePattern(true, "_Stuff_.txt_", "1Stuff3.txt4");
            TestSqlLikePattern(false, "_Stuff_.txt_", "1Stuff.txt4");
            TestSqlLikePattern(false, "_Stuff_.txt_", "1Stuff3.txt");
            TestSqlLikePattern(false, "_Stuff_.txt_", "Stuff3.txt4");
        }

        public void TestSqlLikePattern(bool expectedResult, string pattern, string testString)
        {
            Assert.AreEqual(expectedResult, testString.Like(pattern));
        }
    }
}
