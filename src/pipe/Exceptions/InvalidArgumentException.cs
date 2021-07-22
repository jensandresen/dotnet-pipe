using System;

namespace pipe.Exceptions
{
    public class InvalidArgumentException : Exception
    {
        public InvalidArgumentException(string message) : base(message)
        {
            
        }
    }
}