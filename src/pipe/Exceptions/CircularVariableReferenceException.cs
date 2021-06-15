using System;

namespace pipe.Exceptions
{
    public class CircularVariableReferenceException : Exception
    {
        public CircularVariableReferenceException(string message) : base(message)
        {
            
        }
    }
}