using System;

namespace pipe.Exceptions
{
    public class StepNotDefinedException : Exception
    {
        public StepNotDefinedException(string message) : base(message)
        {
            
        }
    }
}