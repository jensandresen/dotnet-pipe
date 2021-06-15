using System;

namespace pipe.Shells
{
    public class CommandLineExecutorException : Exception
    {
        public CommandLineExecutorException(string message) : base(message)
        {
            
        }
    }
}