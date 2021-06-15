using System;

namespace pipe.Exceptions
{
    public class MissingRequiredStepException : Exception
    {
        public MissingRequiredStepException(string message) : base(message)
        {
            
        }
    }
}