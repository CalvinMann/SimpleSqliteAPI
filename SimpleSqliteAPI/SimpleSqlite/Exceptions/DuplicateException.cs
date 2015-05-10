using System;

namespace SimpleSqlite.Exceptions
{
    public class DuplicateException : Exception
    {
        public string Name { get; private set; }

        public DuplicateException(string name)
        {
            Name = name;
        }

        public DuplicateException(string name, string message) : base(message)
        {
            Name = name;
        }
    }
}
