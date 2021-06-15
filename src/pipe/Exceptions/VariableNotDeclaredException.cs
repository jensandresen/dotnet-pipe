using System;

namespace pipe.Exceptions
{
    public class VariableNotDeclaredException : Exception
    {
        public VariableNotDeclaredException(string message) : base(message)
        {
            
        }
    }
}