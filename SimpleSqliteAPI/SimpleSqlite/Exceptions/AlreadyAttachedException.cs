using System;

namespace SimpleSqlite.Exceptions
{
    public class AlreadyAttachedException : Exception
    {
        public string Name { get; private set; }

        public AlreadyAttachedException(string name, string message) : base(message)
        {
            Name = name;
        }
    }
}
