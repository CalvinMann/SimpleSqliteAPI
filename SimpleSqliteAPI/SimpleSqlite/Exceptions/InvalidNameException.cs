using System;

namespace SimpleSqlite.Exceptions
{
    public class InvalidNameException : Exception
    {
        public string Name { get; private set; }

        public InvalidNameException(string name, string message) : base(message)
        {
            Name = name;
        }
    }
}
