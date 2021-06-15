using System;

namespace pipe.Exceptions
{
    public class StepSelfReferencesException : Exception
    {
        public StepSelfReferencesException(string message) : base(message)
        {
            
        }
    }
}