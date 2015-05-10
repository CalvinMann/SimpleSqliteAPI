using System;

namespace SimpleSqlite.Exceptions
{
    public class InvalidTypeException : Exception
    {
        public Type Type { get; private set; }

        public InvalidTypeException(Type type, string message) 
            : base(message)
        {
            Type = type;
        }
    }
}
