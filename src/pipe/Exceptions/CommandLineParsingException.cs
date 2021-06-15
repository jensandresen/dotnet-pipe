using System;

namespace pipe.Exceptions
{
    public class CommandLineParsingException : Exception
    {
        public CommandLineParsingException(string message) : base(message)
        {
            
        }
    }
}