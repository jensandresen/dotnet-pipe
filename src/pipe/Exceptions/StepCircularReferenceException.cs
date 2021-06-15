using System;

namespace pipe.Exceptions
{
    public class StepCircularReferenceException : Exception
    {
        public StepCircularReferenceException(string message) : base(message)
        {
            
        }
    }
}